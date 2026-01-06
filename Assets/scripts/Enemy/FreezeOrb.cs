using UnityEngine;

/// <summary>
/// 冰凍球投射物（階段1攻擊）
/// 碰到玩家後會凍結玩家，支援無敵星星免疫
/// </summary>
public class FreezeOrb : MonoBehaviour
{
    [Header("基本屬性")]
    [Tooltip("冰凍持續時間")]
    public float freezeDuration = 3f;

    [Tooltip("飛行速度（由Boss設定）")]
    public float speed = 6f;

    [Tooltip("生命週期（秒）")]
    public float lifetime = 6f;

    [Header("視覺效果")]
    [Tooltip("冰凍球顏色")]
    public Color freezeOrbColor = new Color(0.4f, 0.8f, 1f, 0.9f);

    [Tooltip("發光顏色")]
    public Color glowColor = new Color(0.7f, 1f, 1f, 1f);

    [Tooltip("是否旋轉")]
    public bool enableRotation = true;

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 180f;

    [Tooltip("脈動速度")]
    public float pulseSpeed = 3f;

    [Tooltip("脈動強度")]
    public float pulseIntensity = 0.3f;

    [Header("音效")]
    [Tooltip("擊中音效")]
    public AudioClip hitSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private float startTime;

    void Start()
    {
        // 初始化組件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 設定物理
        if (rb != null)
        {
            rb.gravityScale = 0;
        }

        // 設定顏色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = freezeOrbColor;
        }

        startTime = Time.time;

        // 自動銷毀
        Destroy(gameObject, lifetime);

        Debug.Log("[FreezeOrb] 冰凍球生成");
    }

    void Update()
    {
        // 旋轉效果
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // 脈動發光效果
        if (spriteRenderer != null)
        {
            float pulse = Mathf.Sin((Time.time - startTime) * pulseSpeed) * pulseIntensity;
            Color currentColor = Color.Lerp(freezeOrbColor, glowColor, 0.5f + pulse);
            spriteRenderer.color = currentColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 碰到玩家
        if (other.CompareTag("Player"))
        {
            // ========== 檢查無敵狀態 ==========
            InvincibilityController invincibility = other.GetComponent<InvincibilityController>();
            if (invincibility != null && invincibility.IsInvincible())
            {
                Debug.Log("[FreezeOrb] 玩家處於無敵狀態，冰凍球無效！");
                Destroy(gameObject);
                return;
            }

            // 凍結玩家
            PlayerFreezeEffect freezeEffect = other.GetComponent<PlayerFreezeEffect>();
            if (freezeEffect != null)
            {
                freezeEffect.Freeze(freezeDuration);
                Debug.Log($"[FreezeOrb] 冰凍球擊中玩家，凍結 {freezeDuration} 秒");
            }

            // 播放音效
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // 銷毀冰凍球
            Destroy(gameObject);
        }
        // 碰到牆壁或地面（非Boss）
        else if (!other.CompareTag("Boss") && !other.isTrigger)
        {
            Debug.Log("[FreezeOrb] 冰凍球碰到障礙物");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設定冰凍球移動方向和速度
    /// </summary>
    public void SetVelocity(Vector2 direction, float newSpeed)
    {
        if (rb != null)
        {
            speed = newSpeed;
            rb.velocity = direction.normalized * speed;
        }
    }
}