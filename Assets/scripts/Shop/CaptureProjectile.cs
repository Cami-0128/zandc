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

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Debug.Log($"[射線初始化] 位置：{transform.position}，方向：{direction}，碰撞體大小：{col.size}，IsTrigger：{col.isTrigger}");
        }
        else
        {
            Debug.LogError("[射線初始化] 找不到 BoxCollider2D！");
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[射線初始化] 找不到 Rigidbody2D！");
        }
    }

    void Update()
    {
        if (hasHit) return;

        float moveAmount = speed * Time.deltaTime;
        transform.position += Vector3.right * direction * moveAmount;
        traveledDistance += moveAmount;

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, transform.position);
        }

        if (Time.frameCount % 30 == 0)
        {
            Debug.Log($"[射線移動] 當前位置：{transform.position}，已移動：{traveledDistance:F2}");
        }

        if (traveledDistance >= maxDistance)
        {
            Debug.Log($"[射線] 已飛行 {traveledDistance:F2} 單位，超過最大距離 {maxDistance}，銷毀");
            Destroy(gameObject);
        }
    }

    // ⭐ Trigger 碰撞檢測
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[射線 Trigger] ⚡ 碰到物件！名稱：{other.gameObject.name}，Tag：{other.gameObject.tag}，Layer：{LayerMask.LayerToName(other.gameObject.layer)}");

        if (hasHit) return;

        // ✅ 捕捉普通魚
        if (other.CompareTag("Fish"))
        {
            NormalFish normalFish = other.GetComponent<NormalFish>();
            if (normalFish != null)
            {
                Debug.Log($"[射線] ✅ 普通魚 Tag 匹配成功！開始捕捉：{other.gameObject.name}");
                hasHit = true;
                CaptureFish(other.gameObject, "Normal");
                Destroy(gameObject, 0.1f);
                return;
            }

            // ✅ 捕捉特殊魚
            SpecialFish specialFish = other.GetComponent<SpecialFish>();
            if (specialFish != null)
            {
                Debug.Log($"[射線] ✅ 特殊魚 Tag 匹配成功！開始捕捉：{other.gameObject.name}");
                hasHit = true;
                CaptureFish(other.gameObject, "Special");
                Destroy(gameObject, 0.1f);
                return;
            }
        }

        // 【原有】Boss 捕捉判斷
        if (other.gameObject.CompareTag("Boss"))
        {
            Debug.Log($"[射線] ✅ Boss Tag 匹配成功！開始捕捉：{other.gameObject.name}");
            hasHit = true;
            CaptureBoss(other.gameObject);
            Destroy(gameObject, 0.1f);
            return;
        }

        // 【原有】普通敵人捕捉
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Enemy1"))
        {
            Debug.Log($"[射線] ✅ 敵人 Tag 匹配成功！開始捕捉：{other.gameObject.name}");
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

        GameObject collidedObj = collision.gameObject; // ✅ 修正：使用 gameObject 而不是直接調用 GetComponent

        // ✅ 捕捉魚
        if (collidedObj.CompareTag("Fish"))
        {
            NormalFish normalFish = collidedObj.GetComponent<NormalFish>();
            if (normalFish != null)
            {
                Debug.Log($"[射線 Collision] ✅ 普通魚匹配成功！");
                hasHit = true;
                CaptureFish(collidedObj, "Normal");
                Destroy(gameObject, 0.1f);
                return;
            }

            SpecialFish specialFish = collidedObj.GetComponent<SpecialFish>();
            if (specialFish != null)
            {
                Debug.Log($"[射線 Collision] ✅ 特殊魚匹配成功！");
                hasHit = true;
                CaptureFish(collidedObj, "Special");
                Destroy(gameObject, 0.1f);
                return;
            }
        }

        // 【原有】Boss 捕捉判斷
        if (collidedObj.CompareTag("Boss"))
        {
            Debug.Log($"[射線 Collision] ✅ Boss 匹配成功！");
            hasHit = true;
            CaptureBoss(collidedObj);
            Destroy(gameObject, 0.1f);
            return;
        }

        if (collidedObj.CompareTag("Enemy") || collidedObj.CompareTag("Enemy1"))
        {
            Debug.Log($"[射線 Collision] ✅ 敵人匹配成功！");
            hasHit = true;
            CaptureEnemy(collidedObj);
            Destroy(gameObject, 0.1f);
        }
    }

    // ✅ 新增：捕捉魚的方法
    void CaptureFish(GameObject fish, string fishType)
    {
        Debug.Log($"[捕捉技能] 🎣 成功捕捉{fishType}魚：{fish.name}");

        GameObject bubble;

        if (bubblePrefab != null)
        {
            bubble = Instantiate(bubblePrefab, fish.transform.position, Quaternion.identity);
        }
        else
        {
            bubble = CreateDefaultBubble(fish.transform.position);
        }

        CaptureBubble bubbleScript = bubble.GetComponent<CaptureBubble>();
        if (bubbleScript == null)
        {
            bubbleScript = bubble.AddComponent<CaptureBubble>();
        }

        // ✅ 根據魚的類型調用相應的初始化方法
        if (fishType == "Normal")
        {
            bubbleScript.InitializeNormalFish(fish, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
        }
        else if (fishType == "Special")
        {
            bubbleScript.InitializeSpecialFish(fish, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
        }
    }

    // 【原有】捕捉Boss的方法
    void CaptureBoss(GameObject boss)
    {
        Debug.Log($"[捕捉技能] 🎯 成功捕捉Boss：{boss.name}");

        GameObject bubble;

        if (bubblePrefab != null)
        {
            bubble = Instantiate(bubblePrefab, boss.transform.position, Quaternion.identity);
        }
        else
        {
            bubble = CreateDefaultBubble(boss.transform.position);
        }

        CaptureBubble bubbleScript = bubble.GetComponent<CaptureBubble>();
        if (bubbleScript == null)
        {
            bubbleScript = bubble.AddComponent<CaptureBubble>();
        }

        bubbleScript.InitializeBoss(boss, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
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