using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("�l�u�]�w")]
    public float speed = 10f;
    public float damage = 50f;
    public float lifetime = 5f;

    [Header("��ı�ĪG")]
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
            rb.gravityScale = 0f; // �קK�����O�v�T
            rb.freezeRotation = true;
        }

        // �]�w Rigidbody2D �t��
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
        Debug.Log("�]�k�l�u�����ؼСI");
    }
}
