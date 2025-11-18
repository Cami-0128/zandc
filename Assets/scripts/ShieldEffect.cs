using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    [Header("護盾視覺效果")]
    [Tooltip("旋轉速度")]
    public float rotationSpeed = 50f;
    [Tooltip("脈動速度")]
    public float pulseSpeed = 2f;
    [Tooltip("最小縮放")]
    public float minScale = 0.9f;
    [Tooltip("最大縮放")]
    public float maxScale = 1.1f;
    [Tooltip("護盾顏色")]
    public Color shieldColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = shieldColor;
        }
    }

    private void Update()
    {
        // 旋轉效果
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // 脈動效果
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        transform.localScale = baseScale * scale;
    }
}