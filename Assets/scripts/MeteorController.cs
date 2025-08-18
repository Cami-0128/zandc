using UnityEngine;

public class MeteorController : MonoBehaviour
{
    [Header("�k�۳]�w")]
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

        // �Ұʤ��K�ĪG
        if (fireEffect != null)
        {
            fireEffect.Play();
        }

        // �]�w�ͩR�g��
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // ���ʹk��
        if (rb != null && moveDirection != Vector2.zero)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // �]�w���ʤ�V
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

    // �]�w�t��
    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // �]�w�ˮ`
    public void SetDamage(int damageValue)
    {
        damage = damageValue;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �I�쪱�a����
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

        // �I��a������
        if (other.CompareTag("Ground"))
        {
            DestroyMeteor();
        }
    }

    void DestroyMeteor()
    {
        // ����ɤl�ĪG
        if (fireEffect != null)
        {
            fireEffect.Stop();
        }

        // �P���k��
        Destroy(gameObject);
    }
}