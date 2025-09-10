using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class infoUIControllerAnimated : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;        // ���k�������O
    public Button toggleButton;         // �k�W�������s�]��ܭ��O�Ρ^
    public Button closeButton;          // infoPanel�W���e�e���s�]�������O�Ρ^
    public CanvasGroup infoPanelCanvasGroup;  // �Ω�H�J�H�X�ĪG

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.I;  // ����UI������A�w�]�� I

    [Header("Animation Settings")]
    public bool useAnimation = true;
    public float animationDuration = 0.3f;
    public AnimationType animationType = AnimationType.ScaleWithFade;

    [Header("Audio Settings (Optional)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    public enum AnimationType
    {
        Fade,              // �H�J�H�X
        Scale,             // �Y��
        SlideFromTop,      // �q�W��ƤJ
        SlideFromRight,    // �q�k��ƤJ
        ScaleWithFade      // �Y��+�H�J�H�X�զX
    }

    private bool isInfoPanelVisible = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private AudioSource audioSource;

    void Start()
    {
        // �x�s��l�ƭ�
        if (infoPanel != null)
        {
            originalScale = infoPanel.transform.localScale;
            originalPosition = infoPanel.transform.localPosition;

            // ��l����InfoPanel
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

        // �]�m���s�ƥ�
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

        // �۰�����βK�[CanvasGroup
        if (infoPanelCanvasGroup == null && infoPanel != null)
        {
            infoPanelCanvasGroup = infoPanel.GetComponent<CanvasGroup>();
            if (infoPanelCanvasGroup == null)
            {
                infoPanelCanvasGroup = infoPanel.AddComponent<CanvasGroup>();
            }
        }

        // �]�m����
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (openSound != null || closeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // �u���b�S���ʵe�i��ɤ~�T������
        if (isAnimating) return;

        // I��u��}�ҭ��O
        if (Input.GetKeyDown(toggleKey) && !isInfoPanelVisible)
        {
            ShowInfoPanel();
        }

        // ESC���������O
        if (Input.GetKeyDown(KeyCode.Escape) && isInfoPanelVisible)
        {
            HideInfoPanel();
        }
    }

    /// <summary>
    /// ���InfoPanel
    /// </summary>
    public void ShowInfoPanel()
    {
        if (infoPanel == null || isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = true;

        // ����}�ҭ���
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
    /// ����InfoPanel
    /// </summary>
    public void HideInfoPanel()
    {
        if (infoPanel == null || !isInfoPanelVisible || isAnimating) return;

        isInfoPanelVisible = false;

        // ������������
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
        // �P�ɶi���Y��M�H�J�H�X
        Vector3 startScale = show ? Vector3.zero : originalScale;
        Vector3 endScale = show ? originalScale : Vector3.zero;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // �Y��ϥμu�ʮĪG
            float scaleT = show ? EaseOutBack(t) : EaseInBack(t);
            infoPanel.transform.localScale = Vector3.Lerp(startScale, endScale, scaleT);

            // �H�J�H�X�ϥΥ��ƮĪG
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

    // �w�ʨ��
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
    /// ���InfoPanel��e��ܪ��A
    /// </summary>
    public bool IsInfoPanelVisible()
    {
        return isInfoPanelVisible;
    }

    /// <summary>
    /// �����ʵe�����]�i�b�C�����ʺA�եΡ^
    /// </summary>
    public void SetAnimationType(AnimationType newType)
    {
        if (!isAnimating)
        {
            animationType = newType;
        }
    }
}