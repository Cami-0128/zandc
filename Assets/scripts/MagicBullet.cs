using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 10f;
    public float damage = 50f;
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
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            CreateHitEffect();
            Destroy(gameObject);
            return;
        }
        if (other.CompareTag("Ground"))
        {
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        Debug.Log("魔法子彈擊中目標！");
    }
}
