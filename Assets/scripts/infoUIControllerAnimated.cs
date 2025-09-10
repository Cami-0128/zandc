using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class infoUIControllerAnimated : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;        // 玩法說明面板
    public Button toggleButton;         // 右上角的按鈕（顯示面板用）
    public Button closeButton;          // infoPanel上的叉叉按鈕（關閉面板用）
    public CanvasGroup infoPanelCanvasGroup;  // 用於淡入淡出效果

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;  // 切換UI的按鍵，預設為 I

    [Header("Animation Settings")]
    public bool useAnimation = true;
    public float animationDuration = 0.3f;
    public AnimationType animationType = AnimationType.ScaleWithFade;

    [Header("Audio Settings (Optional)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    public enum AnimationType
    {
        Fade,              // 淡入淡出
        Scale,             // 縮放
        SlideFromTop,      // 從上方滑入
        SlideFromRight,    // 從右方滑入
        ScaleWithFade      // 縮放+淡入淡出組合
    }

    private bool isInfoPanelVisible = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private AudioSource audioSource;

    void Start()
    {
        // 儲存原始數值
        if (infoPanel != null)
        {
            originalScale = infoPanel.transform.localScale;
            originalPosition = infoPanel.transform.localPosition;

            // 初始隱藏InfoPanel
            if (useAnimation)
            {
                SetupInitialState();
            }
            else
            {
                infoPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("InfoPanel is not assigned!");
        }

        // 設置按鈕事件
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ShowInfoPanel);
        }
        else
        {
            Debug.LogWarning("Toggle button is not assigned!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideInfoPanel);
        }
        else
        {
            Debug.LogWarning("Close button is not assigned!");
        }

        // 自動獲取或添加CanvasGroup
        if (infoPanelCanvasGroup == null && infoPanel != null)
        {
            infoPanelCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoPanelCanvasGroup == null)
            {
                infoPanelCanvasGroup = infoPanel.AddComponent<CanvasGroup>();
            }
        }

        // 設置音效
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // 只有在沒有動畫進行時才響應按鍵
        if (isAnimating) return;

        // I鍵只能開啟面板
        if (Input.GetKeyDown(toggleKey) && !isInfoPanelVisible)
        {
            ShowInfoPanel();
        }

        // ESC鍵關閉面板
        if (Input.GetKeyDown(KeyCode.Escape) && isInfoPanelVisible)
        {
            HideInfoPanel();
        }
    }

    /// <summary>
    /// 顯示InfoPanel
    /// </summary>
    public void ShowInfoPanel()
    {
        if (infoPanel == null || isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = true;

        // 播放開啟音效
        PlaySound(openSound);

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

    /// <summary>
    /// 隱藏InfoPanel
    /// </summary>
    public void HideInfoPanel()
    {
        if (infoPanel == null || !isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = false;

        // 播放關閉音效
        PlaySound(closeSound);

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
        {
            infoPanel.SetActive(true);
        }

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
        {
            infoPanel.SetActive(false);
        }

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
            elapsed += Time.deltaTime;
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
            elapsed += Time.deltaTime;
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
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = show ? EaseOutQuart(t) : EaseInQuart(t);

            infoPanel.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        infoPanel.transform.localPosition = endPos;
    }

    private IEnumerator AnimateScaleWithFade(bool show)
    {
        // 同時進行縮放和淡入淡出
        Vector3 startScale = show ? Vector3.zero : originalScale;
        Vector3 endScale = show ? originalScale : Vector3.zero;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // 縮放使用彈性效果
            float scaleT = show ? EaseOutBack(t) : EaseInBack(t);
            infoPanel.transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);

            // 淡入淡出使用平滑效果
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
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 緩動函數
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

    /// <summary>
    /// 獲取InfoPanel當前顯示狀態
    /// </summary>
    public bool IsInfoPanelVisible()
    {
        return isInfoPanelVisible;
    }

    /// <summary>
    /// 切換動畫類型（可在遊戲中動態調用）
    /// </summary>
    public void SetAnimationType(AnimationType newType)
    {
        if (!isAnimating)
        {
            animationType = newType;
        }
    }
}