using UnityEngine;

public class HeavyBullet : MonoBehaviour
{
    [Header("攻擊屬性")]
    public float damage = 50f;
    public int manaCost = 10;

    [Header("子彈設定")]
    public float speed = 11f;
    public float lifetime = 5f;

    [Header("子彈外觀")]
    public float bulletScale = 0.6f;
    public Color bulletColor = Color.black;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        transform.localScale = new Vector3(bulletScale, bulletScale, 1f);

        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc == null)
            cc = gameObject.AddComponent<CircleCollider2D>();
        cc.radius = 0.5f * bulletScale;
        cc.isTrigger = true;
    }

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = bulletColor;

        Destroy(gameObject, lifetime);
    }

    public void SetSpeed(float bulletSpeed)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(bulletSpeed, 0f);
    }

    public void SetColor(Color color)
    {
        bulletColor = color;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = color;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boss"))
        {
            var boss = other.GetComponent<BossController2D>();
            if (boss != null)
            {
                boss.TakeDamage(damage, "HeavyBullet");
                Destroy(gameObject);
            }
            return;
        }
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
