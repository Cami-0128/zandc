using System.Collections;
using UnityEngine;

public class SpikesTrap : MonoBehaviour
{
    [Header("尖刺狀態")]
    public float hiddenScale = 0.1f;
    public float extendedScale = 1f;
    public float extendDuration = 3f;
    public float extendSpeed = 0.3f;
    public float retractSpeed = 0.2f;

    [Header("偵測設定")]
    public float detectionRange = 2f;
    public LayerMask playerLayer;

    private Vector3 originalScale;
    private bool isExtended = false;
    private bool isAnimating = false;

    void Start()
    {
        originalScale = transform.localScale;
        Vector3 hiddenState = originalScale;
        hiddenState.y = hiddenScale;
        transform.localScale = hiddenState;
    }

    void Update()
    {
        if (isExtended || isAnimating) return;
        CheckPlayerNearby();
    }

    void CheckPlayerNearby()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, playerLayer);
        if (colliders.Length > 0)
        {
            Debug.Log("[IceSpikeTrap] 玩家靠近，冰刺準備伸長！");
            StartCoroutine(ExtendAndRetract());
        }
    }

    IEnumerator ExtendAndRetract()
    {
        isAnimating = true;
        yield return StartCoroutine(AnimateScale(hiddenScale, extendedScale, extendSpeed));
        isExtended = true;
        yield return new WaitForSeconds(extendDuration);
        yield return StartCoroutine(AnimateScale(extendedScale, hiddenScale, retractSpeed));
        isExtended = false;
        isAnimating = false;
    }

    IEnumerator AnimateScale(float fromScale, float toScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float currentScaleY = Mathf.Lerp(fromScale, toScale, progress);
            Vector3 newScale = originalScale;
            newScale.y = currentScaleY;
            transform.localScale = newScale;
            yield return null;
        }
        Vector3 finalScale = originalScale;
        finalScale.y = toScale;
        transform.localScale = finalScale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}