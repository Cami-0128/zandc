using UnityEngine;

/// <summary>
/// 魚的專用子彈 - 支援無敵星星檢測
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

        // 設定碰撞體
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        circleCollider.radius = 0.25f;
        circleCollider.isTrigger = true;

        Debug.Log($"[FishBullet] Awake 完成");
    }

    void Start()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
            Debug.Log($"[FishBullet] Start 設置速度: {rb.velocity}");
        }
    }

    void Update()
    {
        if (Time.time - spawnTime > lifetime)
        {
            Debug.Log($"[FishBullet] ⏰ 子彈因過期而銷毀");
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 newDirection, float newSpeed = -1f)
    {
        direction = newDirection.normalized;

        if (newSpeed > 0)
        {
            speed = newSpeed;
        }

        if (rb != null)
        {
            rb.velocity = direction * speed;
            Debug.Log($"[FishBullet] SetDirection: {direction}, 速度={speed}");
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        Debug.Log($"[FishBullet] 碰撞: {collision.gameObject.name} (Tag: {collision.tag})");

        // ✅ 忽略 WaterZone
        if (collision.gameObject.name == "WaterZone" || collision.CompareTag("Water"))
        {
            Debug.Log("[FishBullet] ✓ 忽略水域");
            return;
        }

        // 不傷害其他魚
        if (collision.CompareTag("Fish"))
        {
            Debug.Log("[FishBullet] 碰到魚，忽略");
            return;
        }

        // ✅ 傷害玩家（檢測無敵）
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null && !player.isDead)
            {
                // ✅ 檢測無敵星星
                InvincibilityController invincibility = player.GetComponent<InvincibilityController>();
                bool isInvincible = invincibility != null && invincibility.IsInvincible();

                if (isInvincible)
                {
                    Debug.Log($"[FishBullet] ⭐ 玩家無敵，子彈被擋住");
                    hasHit = true;
                    Destroy(gameObject);
                    return;
                }

                player.TakeDamage((int)damage);
                Debug.Log($"[FishBullet] ✅ 擊中玩家，造成 {damage} 點傷害");
                hasHit = true;
                Destroy(gameObject);
            }
            return;
        }

        // ✅ 傷害船隻（檢測船上玩家無敵）
        if (collision.CompareTag("Boat"))
        {
            Boat boat = collision.GetComponent<Boat>();
            if (boat != null && !boat.IsSinking())
            {
                // ✅ 如果船上有無敵玩家，保護整個船
                if (boat.HasPlayerOnBoard())
                {
                    PlayerController2D playerOnBoat = boat.GetPlayerOnBoard();
                    if (playerOnBoat != null)
                    {
                        InvincibilityController invincibility = playerOnBoat.GetComponent<InvincibilityController>();
                        bool isInvincible = invincibility != null && invincibility.IsInvincible();

                        if (isInvincible)
                        {
                            Debug.Log($"[FishBullet] ⭐ 船上玩家無敵，保護船隻");
                            hasHit = true;
                            Destroy(gameObject);
                            return;
                        }
                    }
                }

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
            Debug.Log($"[FishBullet] 撞到 {collision.gameObject.name}，銷毀");
            hasHit = true;
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        float aliveTime = Time.time - spawnTime;
        if (!hasHit && aliveTime < lifetime * 0.2f)
        {
            Debug.LogWarning($"[FishBullet] ⚠️ 異常銷毀 ({aliveTime:F3}秒)");
        }
    }
}