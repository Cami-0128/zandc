using UnityEngine;

public class MeteorController : MonoBehaviour
{
    [Header("隕石設定")]
    public float moveSpeed = 8f;
    public int damage = 30;
    public float lifetime = 10f;
    public ParticleSystem fireEffect;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool hasHitPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 10秒後自動銷毀
        Destroy(gameObject, lifetime);

        Debug.Log("隕石已創建");
    }

    void Update()
    {
        // 移動隕石
        if (rb != null)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // 設定隕石移動方向
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Debug.Log($"隕石方向設為: {moveDirection}");
    }

    // 碰撞檢測
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"隕石碰到: {other.name}");

        // 碰到玩家
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            // 尋找玩家的血量控制腳本
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"對玩家造成 {damage} 點傷害！");
            }

            // 銷毀隕石
            Destroy(gameObject);
        }
        // 碰到地面
        else if (other.CompareTag("Ground"))
        {
            Debug.Log("隕石碰到地面，銷毀");
            Destroy(gameObject);
        }
    }
}