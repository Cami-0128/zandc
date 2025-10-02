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
        // === 關鍵：檢查玩家是否死亡 ===
        if (playerController != null && playerController.isDead)
        {
            return; // 死亡後直接返回，不執行任何操作
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

    void CastMagic()
    {
        // 再次檢查是否死亡（雙重保險）
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (currentMana < attackManaCost)
        {
            Debug.Log("魔力不足！當前魔力: " + currentMana + "/" + maxMana);
            return;
        }

        if (magicBulletPrefab == null)
        {
            Debug.LogError("未設定魔法子彈Prefab！");
            return;
        }

        ConsumeMana(attackManaCost);

        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

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

    private void ConsumeMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        lastManaUseTime = Time.time;
        Debug.Log("消耗 " + amount + " MP，剩餘: " + currentMana + "/" + maxMana);
    }

    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // === 死亡時停止回魔 ===
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
            Debug.Log("手動回魔冷卻中！剩餘時間: " + remainingCooldown.ToString("F1") + " 秒");
            return;
        }

        if (currentMana >= maxMana)
        {
            Debug.Log("魔力已滿！");
            return;
        }

        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;

        lastManualRestoreTime = Time.time;
        Debug.Log("手動回復 " + actualRestored + " MP，當前魔力: " + currentMana + "/" + maxMana);
    }

    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;
        Debug.Log("回復 " + actualRestored + " MP，當前魔力: " + currentMana + "/" + maxMana);
    }

    public int GetCurrentMana()
    {
        return currentMana;
    }

    public float GetManaPercentage()
    {
        return (float)currentMana / maxMana;
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
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        float actualHealed = currentHealth - oldHealth;
        Debug.Log("主角回復 " + actualHealed + " 血量，當前血量: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("主角死亡！");
    }

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