using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public GameObject magicBulletPrefab;
    public Transform firePoint;
    public float magicCost = 5f;

    [Header("音效")]
    public AudioClip magicCastSound;
    private AudioSource audioSource;

    [Header("主角血量")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 如果沒有設定發射點，創建一個
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
        // 檢測M鍵輸入
        if (Input.GetKeyDown(KeyCode.M))
        {
            CastMagic();
        }
    }

    void CastMagic()
    {
        // 檢查是否有足夠血量
        if (currentHealth < magicCost)
        {
            Debug.Log("血量不足，無法施法！");
            return;
        }

        // 消耗血量
        currentHealth -= magicCost;
        currentHealth = Mathf.Max(0, currentHealth);

        // 播放音效
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // 創建魔法子彈
        if (magicBulletPrefab != null)
        {
            Vector3 spawnPosition = firePoint.position;
            Vector3 direction = transform.localScale.x > 0 ? Vector3.right : Vector3.left;

            GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

            // 設定子彈方向
            if (direction == Vector3.left)
            {
                bullet.transform.localScale = new Vector3(-1, 1, 1);
            }
        }

        Debug.Log($"施放魔法！剩餘血量: {currentHealth}");
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
        Debug.Log("主角死亡！");
        // 這裡添加死亡邏輯
    }
}
