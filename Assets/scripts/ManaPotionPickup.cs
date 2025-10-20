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

    [Header("�\��]�w")]
    [Tooltip("�O�_�ҥ��]�O���P�_�A�Y�]�O���h���B��")]
    public bool useCheckManaFull = true;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = potionColor;
        }

        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (enableRotation)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin((Time.time + timeOffset) * floatSpeed) * floatHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerAttack player = other.GetComponent<PlayerAttack>();
        if (player != null)
        {
            if (useCheckManaFull)
            {
                if (player.GetCurrentMana() >= player.maxMana)
                {
                    Debug.Log("[ManaPotionPickup] ���a�]�O�w���A�L�k�B���]�O�Ĥ��I");
                    return; // ���B���Ĥ��A�O�d�D��
                }
            }

            player.RestoreMana(manaRestoreAmount);

            Debug.Log($"[ManaPotionPickup] ���a�B���]�O�Ĥ��A�^�_ {manaRestoreAmount} MP�I");

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
