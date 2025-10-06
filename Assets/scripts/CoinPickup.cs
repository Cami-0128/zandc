using UnityEngine;

/// <summary>
/// �����B���}��
/// ��X�� - �t�X�{���� CoinFlip �M CoinManager
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("�����]�w")]
    [Tooltip("��������")]
    public int value = 1;

    [Header("�۰ʮ���")]
    [Tooltip("�����s�b�ɶ��]��^�A0 = �ä�����")]
    public float lifetime = 10f;

    [Header("���V���a�]�w�]�i��^")]
    [Tooltip("�O�_�۰ʭ��V���a")]
    public bool flyToPlayer = false;

    [Tooltip("����t��")]
    public float flySpeed = 5f;

    [Tooltip("�}�l���V���a������ɶ�")]
    public float flyDelay = 0.5f;

    private Transform playerTransform;
    private bool isFlying = false;

    void Start()
    {
        // �۰ʮ���
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }

        // �p�G�ҥέ��V���a�\��
        if (flyToPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Invoke("StartFlyingToPlayer", flyDelay);
            }
        }
    }

    void Update()
    {
        // ���V���a
        if (isFlying && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * flySpeed * Time.deltaTime;
        }
    }

    void StartFlyingToPlayer()
    {
        isFlying = true;

        // �T�έ��O�M���z�]�p�G�� Rigidbody2D�^
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ��������
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddMoney(value);
            }

            // �P������
            Destroy(gameObject);
        }
    }
}