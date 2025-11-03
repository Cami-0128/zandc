using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public GameObject magicBulletPrefab;
    public GameObject heavyBulletPrefab;
    public Transform firePoint;

    [Header("音效")]
    public AudioClip magicCastSound;
    private AudioSource attackAudioSource;

    [Header("主角血量")]
    public float playerCurrentHealth = 100f;
    public float playerMaxHealth = 100f;

    [Header("魔力系統")]
    public int maxMana = 100;
    [SerializeField] private int currentMana;

    [Header("魔力回復設定")]
    public bool enableManaRegen = false;
    public float manaRegenRate = 5f;
    public float manaRegenDelay = 2f;

    [Header("手動回魔設定")]
    public bool enableManualRestore = true;
    public KeyCode manualRestoreKey = KeyCode.R;
    public int manualRestoreAmount = 30;
    public float manualRestoreCooldown = 5f;

    private float lastManaUseTime;
    private float lastManualRestoreTime = -999f;

    private PlayerController2D playerController;
    private Coroutine manaRegenCoroutine;

    // ========== 新增：按鍵綁定管理器 ==========
    private KeyBindingManager keyManager;

    void Start()
    {
        attackAudioSource = GetComponent<AudioSource>();
        if (attackAudioSource == null)
            attackAudioSource = gameObject.AddComponent<AudioSource>();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = new Vector3(1f, 0f, 0f);
            firePoint = firePointObj.transform;
        }

        playerCurrentHealth = playerMaxHealth;
        currentMana = maxMana;

        playerController = GetComponent<PlayerController2D>();

        // ========== 初始化按鍵管理器 ==========
        keyManager = KeyBindingManager.Instance;
        if (keyManager == null)
        {
            Debug.LogWarning("[PlayerAttack] KeyBindingManager 未找到，使用傳統按鍵");
        }

        if (enableManaRegen)
            manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
    }

    void Update()
    {
        if (playerController != null && playerController.isDead)
            return;

        // ========== 修改：整合自定義按鍵攻擊 ==========
        bool attack1Pressed = false;
        bool attack2Pressed = false;

        if (keyManager != null)
        {
            attack1Pressed = keyManager.GetKeyDown(KeyBindingManager.ActionType.Attack1);
            attack2Pressed = keyManager.GetKeyDown(KeyBindingManager.ActionType.Attack2);
        }
        else
        {
            // 回退到傳統按鍵
            attack1Pressed = Input.GetKeyDown(KeyCode.N);
            attack2Pressed = Input.GetKeyDown(KeyCode.M);
        }

        if (attack1Pressed)
            CastNormalAttack();

        if (attack2Pressed)
            CastHeavyAttack();

        if (enableManualRestore && Input.GetKeyDown(manualRestoreKey))
            ManualRestoreMana();
    }

    void CastNormalAttack()
    {
        if (playerController != null && playerController.isDead)
            return;
        if (magicBulletPrefab == null)
            return;

        MagicBullet bulletData = magicBulletPrefab.GetComponent<MagicBullet>();
        int manaCost = bulletData ? bulletData.manaCost : 5;
        if (currentMana < manaCost)
            return;

        ConsumeMana(manaCost);

        if (magicCastSound != null && attackAudioSource != null)
            attackAudioSource.PlayOneShot(magicCastSound);

        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

        int direction = playerController ? playerController.LastHorizontalDirection : 1;
        bullet.transform.localScale = new Vector3(direction, 1, 1);

        MagicBullet bulletScript = bullet.GetComponent<MagicBullet>();
        if (bulletScript != null)
            bulletScript.speed = Mathf.Abs(bulletScript.speed) * direction;
    }

    void CastHeavyAttack()
    {
        if (playerController != null && playerController.isDead)
            return;
        if (heavyBulletPrefab == null)
            return;

        HeavyBullet prefabData = heavyBulletPrefab.GetComponent<HeavyBullet>();
        int manaCost = prefabData ? prefabData.manaCost : 20;
        if (currentMana < manaCost)
            return;

        ConsumeMana(manaCost);

        if (magicCastSound != null && attackAudioSource != null)
            attackAudioSource.PlayOneShot(magicCastSound);

        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(heavyBulletPrefab, spawnPosition, Quaternion.identity);
        int direction = playerController ? playerController.LastHorizontalDirection : 1;

        float scale = prefabData ? prefabData.bulletScale : 0.6f;
        bullet.transform.localScale = new Vector3(scale * direction, scale, 1);

        HeavyBullet bulletScript = bullet.GetComponent<HeavyBullet>();
        if (bulletScript != null)
        {
            bulletScript.bulletScale = scale;
            bulletScript.SetSpeed(Mathf.Abs(bulletScript.speed) * direction);
            bulletScript.SetColor(Color.black);
        }
    }

    private void ConsumeMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        lastManaUseTime = Time.time;

        ManaBarUI manaBar = FindObjectOfType<ManaBarUI>();
        if (manaBar != null)
        {
            manaBar.UpdateManaBar(currentMana, maxMana);
        }
    }

    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (playerController != null && playerController.isDead)
                continue;

            if (currentMana < maxMana && Time.time - lastManaUseTime >= manaRegenDelay)
            {
                float regenAmount = manaRegenRate * 0.1f;
                currentMana += Mathf.RoundToInt(regenAmount);
                currentMana = Mathf.Min(currentMana, maxMana);
            }
        }
    }

    private void ManualRestoreMana()
    {
        if (playerController != null && playerController.isDead)
            return;
        if (Time.time - lastManualRestoreTime < manualRestoreCooldown)
            return;
        if (currentMana >= maxMana)
            return;

        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        lastManualRestoreTime = Time.time;
    }

    public void RestoreMana(int amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
    }

    public int GetCurrentMana() => currentMana;
    public float GetManaPercentage() => (float)currentMana / maxMana;

    public void SetManaRegen(bool enabled)
    {
        if (enabled && !enableManaRegen)
        {
            enableManaRegen = true;
            if (manaRegenCoroutine == null)
                manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
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
            StopCoroutine(manaRegenCoroutine);
    }

    void OnDestroy()
    {
        if (manaRegenCoroutine != null)
            StopCoroutine(manaRegenCoroutine);
    }
}