using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("攻擊屬性")]
    public float damage = 30f;
    public int manaCost = 5;

    [Header("子彈設定")]
    public float speed = 10f;
    public float lifetime = 5f;

    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f);
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc == null)
            cc = gameObject.AddComponent<CircleCollider2D>();
        cc.radius = 0.4f;
        cc.isTrigger = true;
    }

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = bulletColor;

        float direction = Mathf.Sign(transform.localScale.x);
        rb.velocity = new Vector2(speed * direction, 0f);

        Destroy(gameObject, lifetime);
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
                boss.TakeDamage(damage, "MagicBullet");
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
