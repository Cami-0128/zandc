using UnityEngine;
using System.Collections;

public class FloorController : MonoBehaviour
{
    [Header("消失設定")]
    [Tooltip("是否啟用消失動畫")]
    public bool useAnimation = true;

    [Tooltip("地板消失的方式")]
    public DisappearType disappearType = DisappearType.FadeOut;

    [Tooltip("動畫時間(秒)")]
    public float animationTime = 1f;

    [Tooltip("延遲消失時間(秒)")]
    public float delayTime = 0f;

    [Header("視覺特效")]
    [Tooltip("消失前是否閃爍")]
    public bool blinkBeforeDisappear = false;

    [Tooltip("閃爍次數")]
    public int blinkCount = 3;

    [Tooltip("消失時是否震動")]
    public bool shakeBeforeDisappear = false;

    [Tooltip("震動強度")]
    public float shakeIntensity = 0.1f;

    [Tooltip("是否可以重新出現")]
    public bool canReappear = false;

    [Tooltip("重新出現的延遲時間(秒)")]
    public float reappearDelay = 3f;

    [Header("音效設定(可選)")]
    public AudioClip disappearSound;
    public AudioClip reappearSound;

    private SpriteRenderer spriteRenderer;
    private Collider2D floorCollider;
    private AudioSource audioSource;
    private bool isDisappearing = false;
    private Color originalColor;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    public enum DisappearType
    {
        Instant,        // 立即消失
        FadeOut,        // 淡出
        FallDown,       // 下落
        Shrink          // 縮小
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        floorCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    // 觸發地板消失
    public void TriggerDisappear()
    {
        if (!isDisappearing)
        {
            StartCoroutine(DisappearCoroutine());
        }
    }

    private IEnumerator DisappearCoroutine()
    {
        isDisappearing = true;

        // 閃爍效果
        if (blinkBeforeDisappear && spriteRenderer != null)
        {
            yield return StartCoroutine(BlinkEffect());
        }

        // 震動效果
        if (shakeBeforeDisappear)
        {
            yield return StartCoroutine(ShakeEffect());
        }

        // 延遲
        if (delayTime > 0)
        {
            yield return new WaitForSeconds(delayTime);
        }

        // 播放音效
        if (disappearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(disappearSound);
        }

        // 根據類型執行消失動畫
        if (useAnimation)
        {
            switch (disappearType)
            {
                case DisappearType.Instant:
                    SetFloorActive(false);
                    break;

                case DisappearType.FadeOut:
                    yield return StartCoroutine(FadeOutCoroutine());
                    SetFloorActive(false);
                    break;

                case DisappearType.FallDown:
                    yield return StartCoroutine(FallDownCoroutine());
                    SetFloorActive(false);
                    break;

                case DisappearType.Shrink:
                    yield return StartCoroutine(ShrinkCoroutine());
                    SetFloorActive(false);
                    break;
            }
        }
        else
        {
            SetFloorActive(false);
        }

        // 如果可以重新出現
        if (canReappear)
        {
            yield return new WaitForSeconds(reappearDelay);
            ReappearFloor();
        }
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / animationTime);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    private IEnumerator FallDownCoroutine()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 5f;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / animationTime);

            // 同時淡出
            float alpha = Mathf.Lerp(1f, 0f, elapsed / animationTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }
    }

    private IEnumerator ShrinkCoroutine()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsed / animationTime);
            transform.localScale = startScale * scale;

            yield return null;
        }
    }

    private IEnumerator BlinkEffect()
    {
        float blinkInterval = 0.1f;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator ShakeEffect()
    {
        float elapsed = 0f;
        float shakeDuration = 0.5f;

        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );

            transform.position = originalPosition + randomOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    private void SetFloorActive(bool active)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }

        if (floorCollider != null)
        {
            floorCollider.enabled = active;
        }
    }

    private void ReappearFloor()
    {
        // 重置位置
        transform.position = originalPosition;

        // 重置縮放
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = true;
        }

        SetFloorActive(true);
        isDisappearing = false;

        // 播放重現音效
        if (reappearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reappearSound);
        }
    }

    // 公開方法供其他腳本調用
    public bool IsDisappearing()
    {
        return isDisappearing;
    }
}