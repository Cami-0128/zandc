using UnityEngine;

public class ManaPotionPickup : MonoBehaviour
{
    [Header("�Ĥ��]�w")]
    [Tooltip("�^�_���]�O�q")]
    public int manaRestoreAmount = 30;

    [Header("��ı�ĪG")]
    [Tooltip("�Ĥ����C��")]
    public Color potionColor = new Color(0.3f, 0.5f, 1f); // �Ŧ�

    [Tooltip("�B���ɪ��S��")]
    public GameObject pickupEffectPrefab;

    [Header("����")]
    [Tooltip("�B���ɪ�����")]
    public AudioClip pickupSound;

    [Header("����ʵe")]
    [Tooltip("�O�_�ҥα���ʵe")]
    public bool enableRotation = true;

    [Tooltip("����t��")]
    public float rotationSpeed = 90f;

    [Header("�}�B�ʵe")]
    [Tooltip("�O�_�ҥΤW�U�}�B")]
    public bool enableFloating = true;

    [Tooltip("�}�B����")]
    public float floatHeight = 0.3f;

    [Tooltip("�}�B�t��")]
    public float floatSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        // ��� SpriteRenderer �ó]�w�C��
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = potionColor;
        }

        // �O����l��m
        startPosition = transform.position;

        // �H���ɶ������A���h���Ĥ����P�B
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        // ����ʵe
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // �}�B�ʵe
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �ˬd�O�_�O���a
        PlayerAttack player = other.GetComponent<PlayerAttack>();

        if (player != null)
        {
            // �^�_���a�]�O
            player.RestoreMana(manaRestoreAmount);

            Debug.Log($"[ManaPotionPickup] ���a�B���]�O�Ĥ��A�^�_ {manaRestoreAmount} MP�I");

            // ����B������
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // �ͦ��B���S��
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // �P���Ĥ�
            Destroy(gameObject);
        }
    }
}