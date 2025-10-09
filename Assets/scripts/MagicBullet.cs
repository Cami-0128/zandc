using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("�����ݩ�")]
    public float damage = 30f;
    public int manaCost = 5;

    [Header("�l�u�]�w")]
    public float speed = 10f;
    public float lifetime = 5f;

    [Header("��ı�ĪG")]
    public Color bulletColor = new Color(0.5f, 0.8f, 1f, 1f);

    private Rigidbody2D rb;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = bulletColor;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        float direction = Mathf.Sign(transform.localScale.x);
        rb.velocity = new Vector2(speed * direction, 0f);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, "MagicBullet");
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
        Debug.Log($"[MagicBullet] �����ؼСI�y�� {damage} �I�ˮ`");
    }

    public void SetDamage(float newDamage) => damage = newDamage;

    public void SetManaCost(int cost) => manaCost = cost;

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            rb.velocity = new Vector2(speed * direction, 0f);
        }
    }
    public void SetColor(Color color)
    {
        bulletColor = color;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = color;
    }
}
