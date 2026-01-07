using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 20;
    public float lifetime = 5f;
    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // 旋轉投射物朝向移動方向
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 擊中玩家
        if (collision.CompareTag("Player"))
        {
            PlayerController2D player = collision.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("[BossProjectile] 擊中玩家");
            }
            Destroy(gameObject);
        }
        // 擊中地面或牆壁
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}


