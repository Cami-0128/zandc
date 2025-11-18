using UnityEngine;

/// <summary>
/// 史萊姆包裹特效 - 附加到包裹特效預製物上
/// </summary>
public class SlimeCaptureEffect : MonoBehaviour
{
    [Header("特效設定")]
    [Tooltip("特效顏色")]
    public Color effectColor = new Color(0.2f, 0.8f, 0.3f, 0.7f);
    [Tooltip("旋轉速度")]
    public float rotationSpeed = 180f;
    [Tooltip("脈動速度")]
    public float pulseSpeed = 3f;
    [Tooltip("最小縮放")]
    public float minScale = 0.8f;
    [Tooltip("最大縮放")]
    public float maxScale = 1.2f;
    [Tooltip("是否啟用粒子特效")]
    public bool enableParticles = true;

    private SpriteRenderer spriteRenderer;
    private ParticleSystem particles;
    private Vector3 baseScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particles = GetComponent<ParticleSystem>();
        baseScale = transform.localScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = effectColor;
        }

        if (particles != null && enableParticles)
        {
            var main = particles.main;
            main.startColor = effectColor;
        }
    }

    void Update()
    {
        // 旋轉效果
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // 脈動效果
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        transform.localScale = baseScale * scale;
    }
}