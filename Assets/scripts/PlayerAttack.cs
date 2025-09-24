using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("�����]�w")]
    public GameObject magicBulletPrefab;
    public Transform firePoint;

    [Header("����")]
    public AudioClip magicCastSound;
    private AudioSource audioSource;

    [Header("�D����q")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("����]�w")]
    public KeyCode attackKey = KeyCode.M;

    private PlayerController2D playerController;

    void Start()
    {
        // ����βK�[AudioSource�ե�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // �p�G�S���]�w�o�g�I�A�Ыؤ@��
        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }

        // ��l�Ʀ�q
        currentHealth = maxHealth;

        // ���o PlayerController2D
        playerController = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        // �˴��������J
        if (Input.GetKeyDown(attackKey))
        {
            CastMagic();
        }
    }

    void CastMagic()
    {
        // �ˬd�O�_���l�uPrefab
        if (magicBulletPrefab == null)
        {
            Debug.LogError("���]�w�]�k�l�uPrefab�I");
            return;
        }

        // ���񭵮�
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // �Ы��]�k�l�u
        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

        // �ھڪ��a�̫������V�]�w�l�u��V�A�w�]�V�k (1)
        int direction = 1;
        if (playerController != null)
        {
            direction = playerController.LastHorizontalDirection;
        }

        bullet.transform.localScale = new Vector3(direction, 1, 1);

        MagicBullet bulletScript = bullet.GetComponent<MagicBullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = Mathf.Abs(bulletScript.speed) * direction;
        }

        // Debug.Log("�I���]�k�I�L��q����");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("�D������ " + damage + " �I�ˮ`�A�Ѿl��q: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        Debug.Log("�D���^�_ " + healAmount + " ��q�A��e��q: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("�D�����`�I");
        // �i�H�b�o�̲K�[���`�޿�
    }
}
