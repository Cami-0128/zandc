using UnityEngine;

/// <summary>
/// 魔法子彈攻擊
/// 傷害值和魔力消耗在這裡定義
/// </summary>
public class MagicBullet : MonoBehaviour
{
    [Header("攻擊屬性")]
    [Tooltip("對敵人造成的傷害")]
    public float damage = 50f;

    [Tooltip("所需魔力（由 PlayerAttack 讀取）")]
    public int manaCost = 5;

    [Header("子彈設定")]
    public float speed = 10f;
    public float lifetime = 5f;

    [Header("視覺效果")]
    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Rigidbody2D rb;

    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor;
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 避免受重力影響
            rb.freezeRotation = true;
        }

        // 設定 Rigidbody2D 速度
        float direction = Mathf.Sign(transform.localScale.x);
        rb.velocity = new Vector2(speed * direction, 0f);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 忽略玩家
        if (other.CompareTag("Player"))
        {
            return;
        }

        // 擊中敵人（關鍵：傳遞自己的傷害值）
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, "MagicBullet");
            CreateHitEffect();
            Destroy(gameObject);
            return;
        }

        // 擊中地面
        if (other.CompareTag("Ground"))
        {
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        Debug.Log($"[MagicBullet] 擊中目標！造成 {damage} 點傷害");
        // 未來可以在這裡添加擊中特效
        // 例如：Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }

    // ============================================
    // 公開方法供外部調用（方便動態調整）
    // ============================================

    /// <summary>
    /// 設定傷害值
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// 設定魔力消耗
    /// </summary>
    public void SetManaCost(int cost)
    {
        manaCost = cost;
    }

    /// <summary>
    /// 設定子彈速度
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;

        if (rb != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            rb.velocity = new Vector2(speed * direction, 0f);
        }
    }

    /// <summary>
    /// 設定子彈顏色
    /// </summary>
    public void SetColor(Color color)
    {
        bulletColor = color;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}