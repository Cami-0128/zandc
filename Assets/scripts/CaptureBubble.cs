using System.Collections;
using UnityEngine;

/// <summary>
/// 捕捉圓球 - 包裹敵人並上升
/// </summary>
public class CaptureBubble : MonoBehaviour
{
    private GameObject capturedEnemy;
    private float riseSpeed;
    private float riseHeight;
    private float captureDelay;

    private Vector3 startPosition;
    private bool isRising = false;
    private float captureTime;

    /// <summary>
    /// 初始化泡泡
    /// </summary>
    public void Initialize(GameObject enemy, float speed, float height, float delay)
    {
        capturedEnemy = enemy;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;

        startPosition = transform.position;
        captureTime = Time.time;

        // 讓敵人跟隨泡泡
        if (capturedEnemy != null)
        {
            // 禁用敵人的物理和AI
            DisableEnemy();

            // 設置敵人為泡泡的子物件
            capturedEnemy.transform.SetParent(transform);
            capturedEnemy.transform.localPosition = Vector3.zero;
        }

        // 縮放動畫
        StartCoroutine(AppearAnimation());
    }

    void Update()
    {
        // 等待延遲後開始上升
        if (!isRising && Time.time - captureTime >= captureDelay)
        {
            isRising = true;
        }

        // 上升
        if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // 達到目標高度後銷毀
            if (transform.position.y >= startPosition.y + riseHeight)
            {
                DestroyBubbleAndEnemy();
            }
        }
    }

    /// <summary>
    /// 禁用敵人的組件
    /// </summary>
    void DisableEnemy()
    {
        if (capturedEnemy == null) return;

        // 禁用 Rigidbody2D
        Rigidbody2D rb = capturedEnemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 禁用所有碰撞體
        Collider2D[] colliders = capturedEnemy.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // 禁用敵人AI腳本（如果有的話）
        MonoBehaviour[] scripts = capturedEnemy.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // 只禁用可能是AI控制的腳本
            if (script.GetType().Name.Contains("Enemy") ||
                script.GetType().Name.Contains("AI") ||
                script.GetType().Name.Contains("Controller"))
            {
                script.enabled = false;
            }
        }

        Debug.Log($"[捕捉系統] 已禁用敵人組件：{capturedEnemy.name}");
    }

    /// <summary>
    /// 出現動畫
    /// </summary>
    IEnumerator AppearAnimation()
    {
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    /// <summary>
    /// 銷毀泡泡和敵人
    /// </summary>
    void DestroyBubbleAndEnemy()
    {
        // 播放消失特效（如果需要的話）
        StartCoroutine(DisappearAnimation());
    }

    /// <summary>
    /// 消失動畫
    /// </summary>
    IEnumerator DisappearAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // 淡出效果
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }

            yield return null;
        }

        // 銷毀敵人
        if (capturedEnemy != null)
        {
            Debug.Log($"[捕捉系統] 敵人已被消除：{capturedEnemy.name}");
            Destroy(capturedEnemy);
        }

        // 銷毀泡泡
        Destroy(gameObject);
    }

    /// <summary>
    /// 如果泡泡被破壞，確保敵人也被銷毀
    /// </summary>
    void OnDestroy()
    {
        if (capturedEnemy != null)
        {
            Destroy(capturedEnemy);
        }
    }
}