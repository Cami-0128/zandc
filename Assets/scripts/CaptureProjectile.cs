using System.Collections;
using UnityEngine;

/// <summary>
/// 捕捉射線 - 水平飛行並捕捉敵人
/// </summary>
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

    /// <summary>
    /// 初始化射線參數
    /// </summary>
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

        // 設置 LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    void Update()
    {
        if (hasHit) return;

        // 水平移動
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        // 更新線條終點
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, transform.position);
        }

        // 超過最大距離則銷毀
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // 檢查是否碰到敵人
        if (collision.CompareTag("Enemy") || collision.CompareTag("Enemy1"))
        {
            hasHit = true;
            CaptureEnemy(collision.gameObject);

            // 射線已完成任務，銷毀
            Destroy(gameObject, 0.1f);
        }
    }

    /// <summary>
    /// 捕捉敵人
    /// </summary>
    void CaptureEnemy(GameObject enemy)
    {
        Debug.Log($"[捕捉技能] 成功捕捉敵人：{enemy.name}");

        // 創建捕捉泡泡
        GameObject bubble;

        if (bubblePrefab != null)
        {
            bubble = Instantiate(bubblePrefab, enemy.transform.position, Quaternion.identity);
        }
        else
        {
            // 創建預設泡泡
            bubble = CreateDefaultBubble(enemy.transform.position);
        }

        // 設置泡泡腳本
        CaptureBubble bubbleScript = bubble.GetComponent<CaptureBubble>();
        if (bubbleScript == null)
        {
            bubbleScript = bubble.AddComponent<CaptureBubble>();
        }

        bubbleScript.Initialize(enemy, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
    }

    /// <summary>
    /// 創建預設泡泡
    /// </summary>
    GameObject CreateDefaultBubble(Vector3 position)
    {
        GameObject bubble = new GameObject("CaptureBubble");
        bubble.transform.position = position;
        bubble.transform.localScale = Vector3.one * bubbleSize;

        // 添加視覺效果（圓形精靈）
        SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();

        // 創建圓形紋理
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
                    // 從中心到邊緣的漸變透明度
                    float alpha = bubbleColor.a * (1f - dist / radius) * 0.6f;
                    pixels[y * resolution + x] = new Color(bubbleColor.r, bubbleColor.g, bubbleColor.b, alpha);
                }
                else if (dist < radius + 2)
                {
                    // 邊緣更明顯
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