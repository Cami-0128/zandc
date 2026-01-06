using UnityEngine;

/// <summary>
/// 冰環視覺特效（階段2範圍攻擊）
/// 純視覺效果，傷害由Boss的OverlapCircle處理
/// </summary>
public class IceRingEffect : MonoBehaviour
{
    [Header("視覺效果")]
    [Tooltip("冰環顏色")]
    public Color ringColor = new Color(0.5f, 0.9f, 1f, 0.6f);

    [Tooltip("擴散速度")]
    public float expandSpeed = 2f;

    [Tooltip("最大縮放倍數")]
    public float maxScale = 1.5f;

    [Tooltip("淡出速度")]
    public float fadeSpeed = 2f;

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 90f;

    private SpriteRenderer spriteRenderer;
    private float currentScale = 0.1f;
    private float currentAlpha = 1f;
    private bool isExpanding = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = ringColor;
        }

        // 初始縮放
        transform.localScale = Vector3.one * currentScale;

        Debug.Log("[IceRing] 冰環特效生成");
    }

    void Update()
    {
        // 擴散效果
        if (isExpanding && currentScale < maxScale)
        {
            currentScale += expandSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * currentScale;

            if (currentScale >= maxScale)
            {
                isExpanding = false;
            }
        }
        else if (!isExpanding)
        {
            // 淡出效果
            currentAlpha -= fadeSpeed * Time.deltaTime;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Max(0, currentAlpha);
                spriteRenderer.color = color;
            }

            if (currentAlpha <= 0)
            {
                Destroy(gameObject);
            }
        }

        // 旋轉效果
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}