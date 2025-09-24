using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public GameObject magicBulletPrefab;
    public Transform firePoint;

    [Header("音效")]
    public AudioClip magicCastSound;
    private AudioSource audioSource;

    [Header("主角血量")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("按鍵設定")]
    public KeyCode attackKey = KeyCode.M;

    private PlayerController2D playerController;

    void Start()
    {
        // 獲取或添加AudioSource組件
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

        // 初始化血量
        currentHealth = maxHealth;

        // 取得 PlayerController2D
        playerController = GetComponent<PlayerController2D>();
    }

    void Update()
    {
        // 檢測攻擊鍵輸入
        if (Input.GetKeyDown(attackKey))
        {
            CastMagic();
        }
    }

    void CastMagic()
    {
        // 檢查是否有子彈Prefab
        if (magicBulletPrefab == null)
        {
            Debug.LogError("未設定魔法子彈Prefab！");
            return;
        }

        // 播放音效
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // 創建魔法子彈
        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

        // 根據玩家最後按的方向設定子彈方向，預設向右 (1)
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

        // Debug.Log("施放魔法！無血量消耗");
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("主角受到 " + damage + " 點傷害，剩餘血量: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        Debug.Log("主角回復 " + healAmount + " 血量，當前血量: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("主角死亡！");
        // 可以在這裡添加死亡邏輯
    }
}
