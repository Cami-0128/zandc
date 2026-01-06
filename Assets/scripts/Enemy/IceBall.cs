using UnityEngine;

/// <summary>
/// 冰球投射物 - Boss發射的攻擊彈幕
/// 支援無敵星星免疫機制
/// </summary>
public class IceBall : MonoBehaviour
{
    [Header("基本屬性")]
    [Tooltip("冰球傷害")]
    public int damage = 15;

    [Tooltip("冰球速度（由Boss設定）")]
    public float speed = 12f;

    [Tooltip("生命週期（秒）")]
    public float lifetime = 5f;

    [Header("視覺效果")]
    [Tooltip("冰球顏色")]
    public Color iceBallColor = new Color(0.5f, 0.9f, 1f, 0.8f);

    [Tooltip("是否旋轉")]
    public bool enableRotation = true;

    [Tooltip("旋轉速度")]
    public float rotationSpeed = 360f;

    [Header("音效")]
    [Tooltip("擊中音效")]
    public AudioClip hitSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

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
            spriteRenderer.color = iceBallColor;
        }

        // 自動銷毀
        Destroy(gameObject, lifetime);

        Debug.Log("[IceBall] 冰球生成");
    }

    void Update()
    {
        // 旋轉效果
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
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
                Debug.Log("[IceBall] 玩家處於無敵狀態，冰球無效！");
                Destroy(gameObject);
                return;
            }

            // 造成傷害
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"[IceBall] 冰球擊中玩家，造成 {damage} 點傷害");
            }

            // 播放音效
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            // 銷毀冰球
            Destroy(gameObject);
        }
        // 碰到牆壁或地面（非Boss）
        else if (!other.CompareTag("Boss") && !other.isTrigger)
        {
            Debug.Log("[IceBall] 冰球碰到障礙物");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設定冰球移動方向和速度
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