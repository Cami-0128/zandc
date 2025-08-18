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

        // 10���۰ʾP��
        Destroy(gameObject, lifetime);

        Debug.Log("�k�ۤw�Ы�");
    }

    void Update()
    {
        // ���ʹk��
        if (rb != null)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // �]�w�k�۲��ʤ�V
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        Debug.Log($"�k�ۤ�V�]��: {moveDirection}");
    }

    // �I���˴�
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"�k�۸I��: {other.name}");

        // �I�쪱�a
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            // �M�䪱�a����q����}��
            PlayerController2D player = other.GetComponent<PlayerController2D>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"�缾�a�y�� {damage} �I�ˮ`�I");
            }

            // �P���k��
            Destroy(gameObject);
        }
        // �I��a��
        else if (other.CompareTag("Ground"))
        {
            Debug.Log("�k�۸I��a���A�P��");
            Destroy(gameObject);
        }
    }
}