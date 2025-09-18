using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("�����]�w")]
    public GameObject magicBulletPrefab;
    public Transform firePoint;
    public float magicCost = 5f;

    [Header("����")]
    public AudioClip magicCastSound;
    private AudioSource audioSource;

    [Header("�D����q")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    void Start()
    {
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
    }

    void Update()
    {
        // �˴�M���J
        if (Input.GetKeyDown(KeyCode.M))
        {
            CastMagic();
        }
    }

    void CastMagic()
    {
        // �ˬd�O�_��������q
        if (currentHealth < magicCost)
        {
            Debug.Log("��q�����A�L�k�I�k�I");
            return;
        }

        // ���Ӧ�q
        currentHealth -= magicCost;
        currentHealth = Mathf.Max(0, currentHealth);

        // ���񭵮�
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // �Ы��]�k�l�u
        if (magicBulletPrefab != null)
        {
            Vector3 spawnPosition = firePoint.position;
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

            GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

            // �]�w�l�u��V
            if (direction == Vector3.left)
            {
                bullet.transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        Debug.Log($"�I���]�k�I�Ѿl��q: {currentHealth}");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("�D�����`�I");
        // �o�̲K�[���`�޿�
    }
}
