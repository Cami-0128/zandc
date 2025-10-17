using System.Collections;
using UnityEngine;

public class CaptureProjectile : MonoBehaviour
{
    private int direction;
    private float speed;
    private float maxDistance;
    private GameObject bubblePrefab;
    private float bubbleRiseSpeed;
    private float bubbleRiseHeight;
    private float captureDelay;
    private Color bubbleColor;
    private float bubbleSize;

    private Vector3 startPosition;
    private bool hasHit = false;
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;
    private float traveledDistance = 0f;

    public void Initialize(int dir, float spd, float maxDist, GameObject bubble,
                          float riseSpeed, float riseHeight, float capDelay,
                          Color bColor, float bSize)
    {
        direction = dir;
        speed = spd;
        maxDistance = maxDist;
        bubblePrefab = bubble;
        bubbleRiseSpeed = riseSpeed;
        bubbleRiseHeight = riseHeight;
        captureDelay = capDelay;
        bubbleColor = bColor;
        bubbleSize = bSize;

        startPosition = transform.position;

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        // 確認碰撞體設置
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Debug.Log($"[射線初始化] 位置：{transform.position}，方向：{direction}，碰撞體大小：{col.size}，IsTrigger：{col.isTrigger}");
        }
        else
        {
            Debug.LogError("[射線初始化] 找不到 BoxCollider2D！");
        }

        // 確認 Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[射線初始化] 找不到 Rigidbody2D！");
        }
    }

    void Update()
    {
        if (hasHit) return;

        // 移動
        float moveAmount = speed * Time.deltaTime;
        transform.position += Vector3.right * direction * moveAmount;
        traveledDistance += moveAmount;

        // 更新視覺
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, transform.position);
        }

        // 每 0.5 秒輸出一次位置
        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[射線移動] 當前位置：{transform.position}，已移動：{traveledDistance:F2}");
        }

        // 超過最大距離
        if (traveledDistance >= maxDistance)
        {
            Debug.Log($"[射線] 已飛行 {traveledDistance:F2} 單位，超過最大距離 {maxDistance}，銷毀");
            Destroy(gameObject);
        }
    }

    // ⭐ 最重要：Trigger 碰撞檢測
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[射線 Trigger] ⚡ 碰到物件！名稱：{other.gameObject.name}，Tag：{other.gameObject.tag}，Layer：{LayerMask.LayerToName(other.gameObject.layer)}");

        if (hasHit) return;

        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log($"[射線] ✅ Tag 匹配成功！開始捕捉：{other.gameObject.name}");
            hasHit = true;
            CaptureEnemy(other.gameObject);
            Destroy(gameObject, 0.1f);
        }
        else
        {
            Debug.LogWarning($"[射線] ❌ Tag 不匹配，物件：{other.gameObject.name}，Tag：{other.gameObject.tag}");
        }
    }

    // 備用：普通碰撞
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[射線 Collision] 碰到物件：{collision.gameObject.name}");

        if (hasHit) return;

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log($"[射線 Collision] ✅ 匹配成功！");
            hasHit = true;
            CaptureEnemy(collision.gameObject);
            Destroy(gameObject, 0.1f);
        }
    }

    void CaptureEnemy(GameObject enemy)
    {
        Debug.Log($"[捕捉技能] 🎯 成功捕捉敵人：{enemy.name}");

        GameObject bubble;

        if (bubblePrefab != null)
        {
            bubble = Instantiate(bubblePrefab, enemy.transform.position, Quaternion.identity);
        }
        else
        {
            bubble = CreateDefaultBubble(enemy.transform.position);
        }

        CaptureBubble bubbleScript = bubble.GetComponent<CaptureBubble>();
        if (bubbleScript == null)
        {
            bubbleScript = bubble.AddComponent<CaptureBubble>();
        }

        bubbleScript.Initialize(enemy, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
    }

    GameObject CreateDefaultBubble(Vector3 position)
    {
        GameObject bubble = new GameObject("CaptureBubble");
        bubble.transform.position = position;
        bubble.transform.localScale = Vector3.one * bubbleSize;

        SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();

        int resolution = 128;
        Texture2D tex = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f - 4;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < radius)
                {
                    float alpha = bubbleColor.a * (1f - dist / radius) * 0.6f;
                    pixels[y * resolution + x] = new Color(bubbleColor.r, bubbleColor.g, bubbleColor.b, alpha);
                }
                else if (dist < radius + 2)
                {
                    pixels[y * resolution + x] = new Color(bubbleColor.r, bubbleColor.g, bubbleColor.b, bubbleColor.a * 0.8f);
                }
                else
                {
                    pixels[y * resolution + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), 100);
        sr.sprite = sprite;
        sr.sortingOrder = 10;

        return bubble;
    }
}