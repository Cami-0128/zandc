using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家攻擊系統
/// 自動讀取攻擊技能的傷害值和魔力消耗
/// </summary>
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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }

        currentHealth = maxHealth;
        currentMana = maxMana;

        playerController = GetComponent<PlayerController2D>();

        if (enableManaRegen)
        {
            manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
        }
    }

    void Update()
    {
        // 檢查玩家是否死亡
        if (playerController != null && playerController.isDead)
        {
            return;
        }

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

    /// <summary>
    /// 施放魔法（自動讀取技能的魔力消耗）
    /// </summary>
    void CastMagic()
    {
        // 再次檢查是否死亡
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (magicBulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] 未設定魔法子彈 Prefab！");
            return;
        }

        // === 關鍵：從 Prefab 讀取魔力消耗 ===
        MagicBullet bulletData = magicBulletPrefab.GetComponent<MagicBullet>();
        int manaCost = 5; // 預設值

        if (bulletData != null)
        {
            manaCost = bulletData.manaCost;
        }

        // 檢查魔力是否足夠
        if (currentMana < manaCost)
        {
            Debug.Log($"[PlayerAttack] 魔力不足！需要 {manaCost} MP，當前 {currentMana} MP");
            return;
        }

        // 消耗魔力
        ConsumeMana(manaCost);

        // 播放音效
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // 生成子彈
        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

        // 設定方向
        int direction = 1;
        if (playerController != null)
        {
            direction = playerController.LastHorizontalDirection;
        }
        bullet.transform.localScale = new Vector3(direction, 1, 1);

        // 設定子彈速度
        MagicBullet bulletScript = bullet.GetComponent<MagicBullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = Mathf.Abs(bulletScript.speed) * direction;
        }

        Debug.Log($"[PlayerAttack] 施放 MagicBullet！消耗 {manaCost} MP，造成 {(bulletData != null ? bulletData.damage : 0)} 點傷害");
    }

    /// <summary>
    /// 消耗魔力
    /// </summary>
    private void ConsumeMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        lastManaUseTime = Time.time;
        Debug.Log($"[PlayerAttack] 消耗 {amount} MP，剩餘: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// 魔力自動回復協程
    /// </summary>
    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // 死亡時停止回魔
            if (playerController != null && playerController.isDead)
            {
                continue;
            }

            if (currentMana < maxMana)
            {
                if (Time.time - lastManaUseTime >= manaRegenDelay)
                {
                    float regenAmount = manaRegenRate * 0.1f;
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
        // 死亡時無法手動回魔
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (Time.time - lastManualRestoreTime < manualRestoreCooldown)
        {
            float remainingCooldown = manualRestoreCooldown - (Time.time - lastManualRestoreTime);
            Debug.Log($"[PlayerAttack] 手動回魔冷卻中！剩餘時間: {remainingCooldown:F1} 秒");
            return;
        }

        if (currentMana >= maxMana)
        {
            Debug.Log("[PlayerAttack] 魔力已滿！");
            return;
        }

        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;

        lastManualRestoreTime = Time.time;
        Debug.Log($"[PlayerAttack] 手動回復 {actualRestored} MP，當前魔力: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// 回復魔力（供外部調用）
    /// </summary>
    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;
        Debug.Log($"[PlayerAttack] 回復 {actualRestored} MP，當前魔力: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// 獲取當前魔力值
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
    /// 玩家受到傷害
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"[PlayerAttack] 主角受到 {damage} 點傷害，剩餘血量: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 回復血量
    /// </summary>
    public void Heal(float healAmount)
    {
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        float actualHealed = currentHealth - oldHealth;
        Debug.Log($"[PlayerAttack] 主角回復 {actualHealed} 血量，當前血量: {currentHealth}");
    }

    void Die()
    {
        Debug.Log("[PlayerAttack] 主角死亡！");
    }

    /// <summary>
    /// 設定魔力回復開關
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
        if (manaRegenCoroutine != null)
        {
            StopCoroutine(manaRegenCoroutine);
        }
    }

    void OnDestroy()
    {
        if (manaRegenCoroutine != null)
        {
            StopCoroutine(manaRegenCoroutine);
        }
    }
}