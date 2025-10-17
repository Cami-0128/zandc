using System.Collections;
using UnityEngine;

public class CaptureBubble : MonoBehaviour
{
    private GameObject capturedEnemy;
    private float riseSpeed;
    private float riseHeight;
    private float captureDelay;

    private Vector3 startPosition;
    private bool isRising = false;
    private float captureTime;

    public void Initialize(GameObject enemy, float speed, float height, float delay)
    {
        capturedEnemy = enemy;
        riseSpeed = speed;
        riseHeight = height;
        captureDelay = delay;

        startPosition = transform.position;
        captureTime = Time.time;

        Debug.Log($"[圓球] 初始化，捕捉敵人：{enemy.name}");

        if (capturedEnemy != null)
        {
            DisableEnemy();
            capturedEnemy.transform.SetParent(transform);
            capturedEnemy.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(AppearAnimation());
    }

    void Update()
    {
        if (!isRising && Time.time - captureTime >= captureDelay)
        {
            isRising = true;
            Debug.Log("[圓球] 開始上升");
        }

        if (isRising)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            if (transform.position.y >= startPosition.y + riseHeight)
            {
                Debug.Log("[圓球] 達到目標高度，準備銷毀");
                DestroyBubbleAndEnemy();
            }
        }
    }

    void DisableEnemy()
    {
        if (capturedEnemy == null) return;

        Rigidbody2D rb = capturedEnemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D[] colliders = capturedEnemy.GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        MonoBehaviour[] scripts = capturedEnemy.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script.GetType().Name.Contains("Enemy") ||
                script.GetType().Name.Contains("AI") ||
                script.GetType().Name.Contains("Controller"))
            {
                script.enabled = false;
            }
        }

        Debug.Log($"[捕捉系統] 已禁用敵人組件：{capturedEnemy.name}");
    }

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

    void DestroyBubbleAndEnemy()
    {
        StartCoroutine(DisappearAnimation());
    }

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

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - t;
                sr.color = c;
            }

            yield return null;
        }

        if (capturedEnemy != null)
        {
            Debug.Log($"[捕捉系統] 敵人已被消除：{capturedEnemy.name}");
            Destroy(capturedEnemy);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (capturedEnemy != null)
        {
            Destroy(capturedEnemy);
        }
    }
}