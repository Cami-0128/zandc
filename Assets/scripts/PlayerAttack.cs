using System.Collections;
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

    [Header("魔力系統")]
    [Tooltip("每次攻擊消耗的魔力")]
    public int attackManaCost = 5;
    [Tooltip("最大魔力值")]
    public int maxMana = 100;
    [SerializeField] private int currentMana;

    [Header("魔力回復設定")]
    [Tooltip("是否啟用自動回魔")]
    public bool enableManaRegen = false;
    [Tooltip("每秒回復的魔力量")]
    public float manaRegenRate = 5f;
    [Tooltip("停止使用魔力後多久開始回復（秒）")]
    public float manaRegenDelay = 2f;

    [Header("手動回魔設定")]
    [Tooltip("是否啟用手動回魔按鍵")]
    public bool enableManualRestore = true;
    [Tooltip("手動回魔按鍵")]
    public KeyCode manualRestoreKey = KeyCode.R;
    [Tooltip("手動回復的魔力量")]
    public int manualRestoreAmount = 30;
    [Tooltip("手動回魔冷卻時間（秒）")]
    public float manualRestoreCooldown = 5f;

    private float lastManaUseTime;
    private float lastManualRestoreTime = -999f;

    [Header("按鍵設定")]
    public KeyCode attackKey = KeyCode.M;

    private PlayerController2D playerController;
    private Coroutine manaRegenCoroutine;

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

        // 初始化血量和魔力
        currentHealth = maxHealth;
        currentMana = maxMana;

        // 取得 PlayerController2D
        playerController = GetComponent<PlayerController2D>();

        // 啟動自動回魔協程（如果啟用）
        if (enableManaRegen)
        {
            manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
        }
    }

    void Update()
    {
        // 檢測攻擊鍵輸入
        if (Input.GetKeyDown(attackKey))
        {
            CastMagic();
        }

        // 檢測手動回魔按鍵
        if (enableManualRestore && Input.GetKeyDown(manualRestoreKey))
        {
            ManualRestoreMana();
        }
    }

    void CastMagic()
    {
        // 檢查魔力是否足夠
        if (currentMana < attackManaCost)
        {
            Debug.Log("魔力不足！當前魔力: " + currentMana + "/" + maxMana);
            return;
        }

        // 檢查是否有子彈Prefab
        if (magicBulletPrefab == null)
        {
            Debug.LogError("未設定魔法子彈Prefab！");
            return;
        }

        // 消耗魔力
        ConsumeMana(attackManaCost);

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
    }

    /// <summary>
    /// 消耗魔力（內部方法）
    /// </summary>
    private void ConsumeMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        lastManaUseTime = Time.time;
        Debug.Log("消耗 " + amount + " MP，剩餘: " + currentMana + "/" + maxMana);
    }

    /// <summary>
    /// 自動回復魔力協程
    /// </summary>
    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // 每0.1秒檢查一次

            if (currentMana < maxMana)
            {
                // 檢查是否過了延遲時間
                if (Time.time - lastManaUseTime >= manaRegenDelay)
                {
                    float regenAmount = manaRegenRate * 0.1f; // 0.1秒的回復量
                    currentMana += Mathf.RoundToInt(regenAmount);
                    currentMana = Mathf.Min(currentMana, maxMana);
                }
            }
        }
    }

    /// <summary>
    /// 手動回復魔力
    /// </summary>
    private void ManualRestoreMana()
    {
        // 檢查冷卻時間
        if (Time.time - lastManualRestoreTime < manualRestoreCooldown)
        {
            float remainingCooldown = manualRestoreCooldown - (Time.time - lastManualRestoreTime);
            Debug.Log("手動回魔冷卻中！剩餘時間: " + remainingCooldown.ToString("F1") + " 秒");
            return;
        }

        // 檢查是否已滿魔
        if (currentMana >= maxMana)
        {
            Debug.Log("魔力已滿！");
            return;
        }

        // 回復魔力
        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;

        lastManualRestoreTime = Time.time;
        Debug.Log("手動回復 " + actualRestored + " MP，當前魔力: " + currentMana + "/" + maxMana);
    }

    /// <summary>
    /// 回復魔力（給藥水或其他道具使用）
    /// </summary>
    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;
        Debug.Log("回復 " + actualRestored + " MP，當前魔力: " + currentMana + "/" + maxMana);
    }

    /// <summary>
    /// 獲取當前魔力（給UI使用）
    /// </summary>
    public int GetCurrentMana()
    {
        return currentMana;
    }

    /// <summary>
    /// 獲取魔力百分比
    /// </summary>
    public float GetManaPercentage()
    {
        return (float)currentMana / maxMana;
    }

    /// <summary>
    /// 受到傷害
    /// </summary>
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

    /// <summary>
    /// 回復血量（給回血藥水使用）
    /// </summary>
    public void Heal(float healAmount)
    {
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        float actualHealed = currentHealth - oldHealth;
        Debug.Log("主角回復 " + actualHealed + " 血量，當前血量: " + currentHealth);
    }

    /// <summary>
    /// 死亡
    /// </summary>
    void Die()
    {
        Debug.Log("主角死亡！");
        // 可以在這裡添加死亡邏輯
        // 例如：播放死亡動畫、禁用玩家控制、顯示遊戲結束畫面等
    }

    /// <summary>
    /// 動態啟用/禁用自動回魔
    /// </summary>
    public void SetManaRegen(bool enabled)
    {
        if (enabled && !enableManaRegen)
        {
            enableManaRegen = true;
            if (manaRegenCoroutine == null)
            {
                manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
            }
        }
        else if (!enabled && enableManaRegen)
        {
            enableManaRegen = false;
            if (manaRegenCoroutine != null)
            {
                StopCoroutine(manaRegenCoroutine);
                manaRegenCoroutine = null;
            }
        }
    }

    void OnDisable()
    {
        // 清理協程
        if (manaRegenCoroutine != null)
        {
            StopCoroutine(manaRegenCoroutine);
        }
    }

    void OnDestroy()
    {
        // 清理協程
        if (manaRegenCoroutine != null)
        {
            StopCoroutine(manaRegenCoroutine);
        }
    }
}