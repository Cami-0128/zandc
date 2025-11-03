using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 資訊 UI 控制器 - 帶動畫效果
/// </summary>
public class infoUIControllerAnimated : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public Button toggleButton;
    public Button closeButton;
    public CanvasGroup infoPanelCanvasGroup;

    [Header("InfoPanel 腳本")]
    public InfoPanel infoPanelScript; // InfoPanel.cs 腳本

    [Header("按鍵設定")]
    public KeyBindingUI keyBindingUI; // 按鍵設定 UI

    [Header("Animation Settings")]
    public bool useAnimation = true;
    public float animationDuration = 0.3f;
    public AnimationType animationType = AnimationType.ScaleWithFade;

    [Header("Audio Settings (Optional)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    public enum AnimationType
    {
        Fade,
        Scale,
        SlideFromTop,
        SlideFromRight,
        ScaleWithFade
    }

    private bool isInfoPanelVisible = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private AudioSource infoAudioSource;
    private KeyBindingManager keyManager;

    void Start()
    {
        keyManager = KeyBindingManager.Instance;

        if (infoPanel != null)
        {
            originalScale = infoPanel.transform.localScale;
            originalPosition = infoPanel.transform.localPosition;

            if (useAnimation)
            {
                SetupInitialState();
            }
            else
            {
                infoPanel.SetActive(false);
            }
        }

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ShowInfoPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideInfoPanel);

        if (infoPanelCanvasGroup == null && infoPanel != null)
        {
            infoPanelCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoPanelCanvasGroup == null)
            {
                infoPanelCanvasGroup = infoPanel.AddComponent<CanvasGroup>();
            }
        }

        infoAudioSource = GetComponent<AudioSource>();
        if (infoAudioSource == null && (openSound != null || closeSound != null))
        {
            infoAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 如果有 InfoPanel 腳本,也連接它
        if (infoPanelScript != null)
        {
            infoPanelScript.keyBindingUI = keyBindingUI;
        }
    }

    void Update()
    {
        if (isAnimating) return;

        bool infoKeyPressed = false;

        // 使用自定義按鍵
        if (keyManager != null)
        {
            infoKeyPressed = keyManager.GetKeyDown(KeyBindingManager.ActionType.OpenInfo);
        }
        else
        {
            infoKeyPressed = Input.GetKeyDown(KeyCode.I);
        }

        if (infoKeyPressed)
        {
            if (!isInfoPanelVisible)
                ShowInfoPanel();
            else
                HideInfoPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isInfoPanelVisible)
        {
            HideInfoPanel();
        }
    }

    public void ShowInfoPanel()
    {
        if (infoPanel == null || isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = true;
        PlaySound(openSound);

        // 暫停遊戲
        Time.timeScale = 0f;

        // 如果有 InfoPanel 腳本,更新文字
        if (infoPanelScript != null)
        {
            infoPanelScript.UpdateInfoText();
        }

        if (useAnimation)
        {
            StartCoroutine(AnimateInfoPanel(true));
        }
        else
        {
            infoPanel.SetActive(true);
        }

        Debug.Log("InfoPanel opened");
    }

    public void HideInfoPanel()
    {
        if (infoPanel == null || !isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = false;
        PlaySound(closeSound);

        // 恢復遊戲
        Time.timeScale = 1f;

        if (useAnimation)
        {
            StartCoroutine(AnimateInfoPanel(false));
        }
        else
        {
            infoPanel.SetActive(false);
        }

        Debug.Log("InfoPanel closed");
    }

    /// <summary>
    /// 重新設定按鍵 (從 Info Panel 的按鈕呼叫)
    /// </summary>
    public void OnReconfigureKeysClicked()
    {
        HideInfoPanel();

        if (keyBindingUI != null)
        {
            keyBindingUI.ShowPanel();
        }
        else
        {
            Debug.LogError("[InfoUI] KeyBindingUI 未設定!");
        }
    }

    private void SetupInitialState()
    {
        infoPanel.SetActive(true);

        switch (animationType)
        {
            case AnimationType.Fade:
                if (infoPanelCanvasGroup != null)
                    infoPanelCanvasGroup.alpha = 0f;
                break;

            case AnimationType.Scale:
                infoPanel.transform.localScale = Vector3.zero;
                break;

            case AnimationType.SlideFromTop:
                infoPanel.transform.localPosition = originalPosition + Vector3.up * Screen.height;
                break;

            case AnimationType.SlideFromRight:
                infoPanel.transform.localPosition = originalPosition + Vector3.right * Screen.width;
                break;

            case AnimationType.ScaleWithFade:
                infoPanel.transform.localScale = Vector3.zero;
                if (infoPanelCanvasGroup != null)
                    infoPanelCanvasGroup.alpha = 0f;
                break;
        }

        infoPanel.SetActive(false);
        isInfoPanelVisible = false;
    }

    private IEnumerator AnimateInfoPanel(bool show)
    {
        isAnimating = true;

        if (show)
            infoPanel.SetActive(true);

        switch (animationType)
        {
            case AnimationType.Fade:
                yield return StartCoroutine(AnimateFade(show));
                break;

            case AnimationType.Scale:
                yield return StartCoroutine(AnimateScale(show));
                break;

            case AnimationType.SlideFromTop:
                yield return StartCoroutine(AnimateSlide(show, Vector3.up));
                break;

            case AnimationType.SlideFromRight:
                yield return StartCoroutine(AnimateSlide(show, Vector3.right));
                break;

            case AnimationType.ScaleWithFade:
                yield return StartCoroutine(AnimateScaleWithFade(show));
                break;
        }

        if (!show)
            infoPanel.SetActive(false);

        isAnimating = false;
    }

    private IEnumerator AnimateFade(bool show)
    {
        if (infoPanelCanvasGroup == null) yield break;

        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            t = show ? EaseOutQuart(t) : EaseInQuart(t);

            infoPanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        infoPanelCanvasGroup.alpha = endAlpha;
    }

    private IEnumerator AnimateScale(bool show)
    {
        Vector3 startScale = show ? Vector3.zero : originalScale;
        Vector3 endScale = show ? originalScale : Vector3.zero;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            t = show ? EaseOutBack(t) : EaseInBack(t);

            infoPanel.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        infoPanel.transform.localScale = endScale;
    }

    private IEnumerator AnimateSlide(bool show, Vector3 direction)
    {
        Vector3 offset = direction * (direction.x != 0 ? Screen.width : Screen.height);
        Vector3 startPos = show ? originalPosition + offset : originalPosition;
        Vector3 endPos = show ? originalPosition : originalPosition + offset;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            t = show ? EaseOutQuart(t) : EaseInQuart(t);

            infoPanel.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        infoPanel.transform.localPosition = endPos;
    }

    private IEnumerator AnimateScaleWithFade(bool show)
    {
        Vector3 startScale = show ? Vector3.zero : originalScale;
        Vector3 endScale = show ? originalScale : Vector3.zero;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;

            float scaleT = show ? EaseOutBack(t) : EaseInBack(t);
            infoPanel.transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);

            float fadeT = show ? EaseOutQuart(t) : EaseInQuart(t);
            if (infoPanelCanvasGroup != null)
            {
                infoPanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, fadeT);
            }

            yield return null;
        }

        infoPanel.transform.localScale = endScale;
        if (infoPanelCanvasGroup != null)
        {
            infoPanelCanvasGroup.alpha = endAlpha;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (infoAudioSource != null && clip != null)
        {
            infoAudioSource.PlayOneShot(clip);
        }
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    private float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }

    private float EaseInQuart(float t)
    {
        return t * t * t * t;
    }

    public bool IsInfoPanelVisible()
    {
        return isInfoPanelVisible;
    }

    public void SetAnimationType(AnimationType newType)
    {
        if (!isAnimating)
        {
            animationType = newType;
        }
    }
}