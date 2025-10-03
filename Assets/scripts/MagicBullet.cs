using UnityEngine;

/// <summary>
/// �]�k�l�u����
/// �ˮ`�ȩM�]�O���Ӧb�o�̩w�q
/// </summary>
public class MagicBullet : MonoBehaviour
{
    [Header("�����ݩ�")]
    [Tooltip("��ĤH�y�����ˮ`")]
    public float damage = 50f;

    [Tooltip("�һ��]�O�]�� PlayerAttack Ū���^")]
    public int manaCost = 5;

    [Header("�l�u�]�w")]
    public float speed = 10f;
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
        // �������a
        if (other.CompareTag("Player"))
        {
            return;
        }

        // �����ĤH�]����G�ǻ��ۤv���ˮ`�ȡ^
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, "MagicBullet");
            CreateHitEffect();
            Destroy(gameObject);
            return;
        }

        // �����a��
        if (other.CompareTag("Ground"))
        {
            CreateHitEffect();
            Destroy(gameObject);
        }
    }

    void CreateHitEffect()
    {
        Debug.Log($"[MagicBullet] �����ؼСI�y�� {damage} �I�ˮ`");
        // ���ӥi�H�b�o�̲K�[�����S��
        // �Ҧp�GInstantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }

    // ============================================
    // ���}��k�ѥ~���եΡ]��K�ʺA�վ�^
    // ============================================

    /// <summary>
    /// �]�w�ˮ`��
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    /// <summary>
    /// �]�w�]�O����
    /// </summary>
    public void SetManaCost(int cost)
    {
        manaCost = cost;
    }

    /// <summary>
    /// �]�w�l�u�t��
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;

        if (rb != null)
        {
            float direction = Mathf.Sign(transform.localScale.x);
            rb.velocity = new Vector2(speed * direction, 0f);
        }
    }

    /// <summary>
    /// �]�w�l�u�C��
    /// </summary>
    public void SetColor(Color color)
    {
        bulletColor = color;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}