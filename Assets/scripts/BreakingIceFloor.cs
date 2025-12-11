using System.Collections;
using UnityEngine;

public class BreakingIceFloor : MonoBehaviour  //破碎冰層
{
    [Header("破碎設定")]
    public float breakDelay = 0.5f;         // 踩上後多久才破碎
    public float fallDuration = 1f;         // 掉落動畫時間
    public float fallDistance = 5f;         // 掉落距離

    [Header("重置設定")]
    public bool shouldReset = true;         // 是否在破碎後重置
    public float resetDelay = 2f;           // 破碎後多久才重置/刪除

    private bool isPlayerOnIce = false;
    private bool isBroken = false;
    private Vector3 originalPosition;
    private Collider2D iceCollider;

    void Start()
    {
        originalPosition = transform.position;
        iceCollider = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            isPlayerOnIce = true;
            Debug.Log("[BreakingIceFloor] 玩家踩上破碎冰層！");
            StartCoroutine(BreakIce());
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnIce = false;
        }
    }

    IEnumerator BreakIce()
    {
        // 等待一段時間後破碎
        yield return new WaitForSeconds(breakDelay);

        if (!isPlayerOnIce) yield break;

        isBroken = true;
        Debug.Log("[BreakingIceFloor] 冰層破碎！");

        // 禁用碰撞器讓玩家掉下去
        if (iceCollider != null)
            iceCollider.enabled = false;

        // 冰層下落動畫
        yield return StartCoroutine(AnimateFall());

        // 根據設定決定是否重置或刪除
        if (shouldReset)
        {
            yield return new WaitForSeconds(resetDelay);
            ResetIce();
        }
        else
        {
            yield return new WaitForSeconds(resetDelay);
            DeleteIce();
        }
    }

    IEnumerator AnimateFall()
    {
        float elapsed = 0f;
        Vector3 startPos = originalPosition;
        Vector3 endPos = originalPosition + Vector3.down * fallDistance;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fallDuration;
            transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        transform.position = endPos;
    }

    void ResetIce()
    {
        Debug.Log("[BreakingIceFloor] 冰層重置");
        isBroken = false;
        isPlayerOnIce = false;
        iceCollider.enabled = true;
        transform.position = originalPosition;
    }

    void DeleteIce()
    {
        Debug.Log("[BreakingIceFloor] 冰層已刪除");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * fallDistance);
    }
}