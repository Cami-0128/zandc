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

        // 啟動火焰效果
        if (fireEffect != null)
        {
            fireEffect.Play();
        }

        // 設定生命週期
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 移動隕石
        if (rb != null && moveDirection != Vector2.zero)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // 設定移動方向
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // 設定速度
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // 設定傷害
    public void SetDamage(int damageValue)
    {
        damage = damageValue;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 碰到玩家扣血
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            DestroyMeteor();
        }

        // 碰到地面消失
        if (other.CompareTag("Ground"))
        {
            DestroyMeteor();
        }
    }

    void DestroyMeteor()
    {
        // 停止粒子效果
        if (fireEffect != null)
        {
            fireEffect.Stop();
        }

        // 銷毀隕石
        Destroy(gameObject);
    }
}