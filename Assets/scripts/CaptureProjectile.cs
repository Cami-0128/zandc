using System.Collections;
using UnityEngine;

/// <summary>
/// �����g�u - ��������î����ĤH
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
    /// ��l�Ʈg�u�Ѽ�
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

        // �]�m LineRenderer
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

        // ��������
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        // ��s�u�����I
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(1, transform.position);
        }

        // �W�L�̤j�Z���h�P��
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        // �ˬd�O�_�I��ĤH
        if (collision.CompareTag("Enemy") || collision.CompareTag("Enemy1"))
        {
            hasHit = true;
            CaptureEnemy(collision.gameObject);

            // �g�u�w�������ȡA�P��
            Destroy(gameObject, 0.1f);
        }
    }

    /// <summary>
    /// �����ĤH
    /// </summary>
    void CaptureEnemy(GameObject enemy)
    {
        Debug.Log($"[�����ޯ�] ���\�����ĤH�G{enemy.name}");

        // �Ыخ����w�w
        GameObject bubble;

        if (bubblePrefab != null)
        {
            bubble = Instantiate(bubblePrefab, enemy.transform.position, Quaternion.identity);
        }
        else
        {
            // �Ыعw�]�w�w
            bubble = CreateDefaultBubble(enemy.transform.position);
        }

        // �]�m�w�w�}��
        CaptureBubble bubbleScript = bubble.GetComponent<CaptureBubble>();
        if (bubbleScript == null)
        {
            bubbleScript = bubble.AddComponent<CaptureBubble>();
        }

        bubbleScript.Initialize(enemy, bubbleRiseSpeed, bubbleRiseHeight, captureDelay);
    }

    /// <summary>
    /// �Ыعw�]�w�w
    /// </summary>
    GameObject CreateDefaultBubble(Vector3 position)
    {
        GameObject bubble = new GameObject("CaptureBubble");
        bubble.transform.position = position;
        bubble.transform.localScale = Vector3.one * bubbleSize;

        // �K�[��ı�ĪG�]��κ��F�^
        SpriteRenderer sr = bubble.AddComponent<SpriteRenderer>();

        // �Ыض�ί��z
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
                    // �q���ߨ���t�����ܳz����
                    float alpha = bubbleColor.a * (1f - dist / radius) * 0.6f;
                    pixels[y * resolution + x] = new Color(bubbleColor.r, bubbleColor.g, bubbleColor.b, alpha);
                }
                else if (dist < radius + 2)
                {
                    // ��t�����
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