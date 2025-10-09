using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("�����]�w")]
    public GameObject magicBulletPrefab;
    public GameObject heavyBulletPrefab;
    public Transform firePoint;

    [Header("����")]
    public AudioClip magicCastSound;
    private AudioSource audioSource;

    [Header("�D����q")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("�]�O�t��")]
    public int maxMana = 100;
    [SerializeField] private int currentMana;

    [Header("�]�O�^�_�]�w")]
    public bool enableManaRegen = false;
    public float manaRegenRate = 5f;
    public float manaRegenDelay = 2f;

    [Header("��ʦ^�]�]�w")]
    public bool enableManualRestore = true;
    public KeyCode manualRestoreKey = KeyCode.R;
    public int manualRestoreAmount = 30;
    public float manualRestoreCooldown = 5f;

    private float lastManaUseTime;
    private float lastManualRestoreTime = -999f;

    [Header("����]�w")]
    public KeyCode normalAttackKey = KeyCode.M;
    public KeyCode heavyAttackKey = KeyCode.N;

    private PlayerController2D playerController;
    private Coroutine manaRegenCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

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
            manaRegenCoroutine = StartCoroutine(ManaRegenerationCoroutine());
    }

    void Update()
    {
        if (playerController != null && playerController.isDead)
            return;

        if (Input.GetKeyDown(normalAttackKey))
            CastNormalAttack();

        if (Input.GetKeyDown(heavyAttackKey))
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

        if (magicCastSound != null && audioSource != null)
            audioSource.PlayOneShot(magicCastSound);

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

        if (magicCastSound != null && audioSource != null)
            audioSource.PlayOneShot(magicCastSound);

        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(heavyBulletPrefab, spawnPosition, Quaternion.identity);
        int direction = playerController ? playerController.LastHorizontalDirection : 1;

        // �]�w�l�u�j�p�P�����V
        float scale = prefabData ? prefabData.bulletScale : 0.6f;
        bullet.transform.localScale = new Vector3(scale, scale, 1);
        HeavyBullet bulletScript = bullet.GetComponent<HeavyBullet>();
        if (bulletScript != null)
        {
            bulletScript.bulletScale = scale;
            bulletScript.SetSpeed(Mathf.Abs(bulletScript.speed) * direction);
            bulletScript.SetColor(Color.black); // �o�̫O�I�]�¦�
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

        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        lastManualRestoreTime = Time.time;
    }

    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
    }

    public int GetCurrentMana() => currentMana;
    public float GetManaPercentage() => (float)currentMana / maxMana;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float healAmount)
    {
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
    }

    void Die()
    {
    }

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
