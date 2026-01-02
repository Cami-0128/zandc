using UnityEngine;

/// <summary>
/// 魚的專用子彈 - 修復版：不與 WaterZone 碰撞
/// </summary>
public class FishBullet : MonoBehaviour
{
    [Header("═══ 子彈屬性 ═══")]
    public float speed = 10f;
    public float lifetime = 8f;
    public float damage = 10f;

    [Header("═══ 視覺效果 ═══")]
    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Vector2 direction = Vector2.right;
    private bool hasHit = false;
    private float spawnTime;

    void Awake()
    {
        spawnTime = Time.time;
        Debug.Log($"[FishBullet] Awake 開始初始化");

        // 設定 Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.isKinematic = false;

        // 設定 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.color = bulletColor;

        // 設定碰撞體（重要：只檢測 Player 和 Boat）
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.radius = 0.25f;
        circleCollider.isTrigger = true;

        Debug.Log($"[FishBullet] Awake 完成 - 位置: {transform.position}");
    }

    void Start()
    {
        // 設定初始速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
            Debug.Log($"[FishBullet] Start 設置速度: {rb.velocity}");
        }
    }

    void Update()
    {
        // 檢查是否超過生命週期
        if (Time.time - spawnTime > lifetime)
        {
            Debug.Log($"[FishBullet] ⏰ 子彈因過期而銷毀 (生存時間: {(Time.time - spawnTime):F2}秒)");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 設定子彈的方向和速度
    /// </summary>
    public void SetDirection(Vector2 newDirection, float newSpeed = -1f)
    {
        direction = newDirection.normalized;

        if (newSpeed > 0)
        {
            speed = newSpeed;
        }

        // 立即設定速度
        if (rb != null)
        {
            rb.velocity = direction * speed;
            Debug.Log($"[FishBullet] SetDirection: 方向={direction}, 速度={speed}, Velocity={rb.velocity}");
        }

        // 根據方向翻轉精靈
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    /// <summary>
    /// 設定傷害值
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        Debug.Log($"[FishBullet] 碰撞檢測: {collision.gameObject.name} (Tag: {collision.tag})");

        // ✅ 修復：忽略 WaterZone（Tag 為空或 "Untagged"）
        if (collision.gameObject.name == "WaterZone" || collision.CompareTag("Water"))
        {
            Debug.Log("[FishBullet] ✓ 忽略水域，子彈繼續飛行");
            return;
        }

        // 不傷害其他魚
        if (collision.CompareTag("Fish"))
        {
            Debug.Log("[FishBullet] 碰到魚，忽略");
            return;
        }

        // 傷害玩家
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null && !player.isDead)
            {
                player.TakeDamage((int)damage);
                Debug.Log($"[FishBullet] ✅ 擊中玩家，造成 {damage} 點傷害");
                hasHit = true;
                Destroy(gameObject);
            }
            return;
        }

        // 傷害船隻
        if (collision.CompareTag("Boat"))
        {
            Boat boat = collision.GetComponent<Boat>();
            if (boat != null && !boat.IsSinking())
            {
                boat.TakeDamage((int)damage);
                Debug.Log($"[FishBullet] ✅ 擊中船隻，造成 {damage} 點傷害");
                hasHit = true;
                Destroy(gameObject);
            }
            return;
        }

        // 撞到其他物體銷毀
        if (!collision.CompareTag("Player") && !collision.CompareTag("Fish") && !collision.CompareTag("Boat"))
        {
            Debug.Log($"[FishBullet] 撞到 {collision.gameObject.name} ({collision.tag})，銷毀子彈");
            hasHit = true;
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        float aliveTime = Time.time - spawnTime;
        if (!hasHit && aliveTime < lifetime * 0.2f)
        {
            Debug.LogWarning($"[FishBullet] ⚠️ 子彈異常銷毀 (生存時間: {aliveTime:F3}秒，預期: {lifetime}秒)");
        }
    }
}