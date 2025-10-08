using UnityEngine;

public class HeavyBullet : MonoBehaviour
{
    [Header("�����ݩ�")]
    public float damage = 50f;
    public int manaCost = 10;

    [Header("�l�u�]�w")]
    public float speed = 11f;
    public float lifetime = 5f;

    [Header("�l�u�~�[")]
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

        // �]�w�j�p
        transform.localScale = new Vector3(bulletScale, bulletScale, 1f);

        // Collider �Y��
        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null)
            cc.radius = 0.5f * bulletScale;
    }

    void Start()
    {
        // �]�w�C��
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = bulletColor;

        // ���Φb�o�̳]�t�סA�t�ץѥ~�� SetSpeed �M�w
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// �~���I�s�A���l�u����
    /// </summary>
    public void SetSpeed(float bulletSpeed)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(bulletSpeed, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, "HeavyBullet");
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
        Debug.Log($"[HeavyBullet] �����ؼСI�y�� {damage} �I�ˮ`");
    }

    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetManaCost(int cost) => manaCost = cost;
    public void SetColor(Color color)
    {
        bulletColor = color;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }
}
