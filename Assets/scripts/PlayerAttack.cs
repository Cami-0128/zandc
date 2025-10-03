using System.Collections;
using UnityEngine;

/// <summary>
/// ���a�����t��
/// �۰�Ū�������ޯ઺�ˮ`�ȩM�]�O����
/// </summary>
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

    [Header("�]�O�t��")]
    [Tooltip("�̤j�]�O��")]
    public int maxMana = 100;
    [SerializeField] private int currentMana;

    [Header("�]�O�^�_�]�w")]
    [Tooltip("�O�_�ҥΦ۰ʦ^�]")]
    public bool enableManaRegen = false;
    [Tooltip("�C��^�_���]�O�q")]
    public float manaRegenRate = 5f;
    [Tooltip("����ϥ��]�O��h�[�}�l�^�_�]��^")]
    public float manaRegenDelay = 2f;

    [Header("��ʦ^�]�]�w")]
    [Tooltip("�O�_�ҥΤ�ʦ^�]����")]
    public bool enableManualRestore = true;
    [Tooltip("��ʦ^�]����")]
    public KeyCode manualRestoreKey = KeyCode.R;
    [Tooltip("��ʦ^�_���]�O�q")]
    public int manualRestoreAmount = 30;
    [Tooltip("��ʦ^�]�N�o�ɶ��]��^")]
    public float manualRestoreCooldown = 5f;

    private float lastManaUseTime;
    private float lastManualRestoreTime = -999f;

    [Header("����]�w")]
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
        // �ˬd���a�O�_���`
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        // �˴��������J
        if (Input.GetKeyDown(attackKey))
        {
            CastMagic();
        }

        // �˴���ʦ^�]����
        if (enableManualRestore && Input.GetKeyDown(manualRestoreKey))
        {
            ManualRestoreMana();
        }
    }

    /// <summary>
    /// �I���]�k�]�۰�Ū���ޯ઺�]�O���ӡ^
    /// </summary>
    void CastMagic()
    {
        // �A���ˬd�O�_���`
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (magicBulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] ���]�w�]�k�l�u Prefab�I");
            return;
        }

        // === ����G�q Prefab Ū���]�O���� ===
        MagicBullet bulletData = magicBulletPrefab.GetComponent<MagicBullet>();
        int manaCost = 5; // �w�]��

        if (bulletData != null)
        {
            manaCost = bulletData.manaCost;
        }

        // �ˬd�]�O�O�_����
        if (currentMana < manaCost)
        {
            Debug.Log($"[PlayerAttack] �]�O�����I�ݭn {manaCost} MP�A��e {currentMana} MP");
            return;
        }

        // �����]�O
        ConsumeMana(manaCost);

        // ���񭵮�
        if (magicCastSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(magicCastSound);
        }

        // �ͦ��l�u
        Vector3 spawnPosition = firePoint.position;
        GameObject bullet = Instantiate(magicBulletPrefab, spawnPosition, Quaternion.identity);

        // �]�w��V
        int direction = 1;
        if (playerController != null)
        {
            direction = playerController.LastHorizontalDirection;
        }
        bullet.transform.localScale = new Vector3(direction, 1, 1);

        // �]�w�l�u�t��
        MagicBullet bulletScript = bullet.GetComponent<MagicBullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = Mathf.Abs(bulletScript.speed) * direction;
        }

        Debug.Log($"[PlayerAttack] �I�� MagicBullet�I���� {manaCost} MP�A�y�� {(bulletData != null ? bulletData.damage : 0)} �I�ˮ`");
    }

    /// <summary>
    /// �����]�O
    /// </summary>
    private void ConsumeMana(int amount)
    {
        currentMana -= amount;
        currentMana = Mathf.Max(0, currentMana);
        lastManaUseTime = Time.time;
        Debug.Log($"[PlayerAttack] ���� {amount} MP�A�Ѿl: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// �]�O�۰ʦ^�_��{
    /// </summary>
    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // ���`�ɰ���^�]
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
    /// ��ʦ^�_�]�O
    /// </summary>
    private void ManualRestoreMana()
    {
        // ���`�ɵL�k��ʦ^�]
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (Time.time - lastManualRestoreTime < manualRestoreCooldown)
        {
            float remainingCooldown = manualRestoreCooldown - (Time.time - lastManualRestoreTime);
            Debug.Log($"[PlayerAttack] ��ʦ^�]�N�o���I�Ѿl�ɶ�: {remainingCooldown:F1} ��");
            return;
        }

        if (currentMana >= maxMana)
        {
            Debug.Log("[PlayerAttack] �]�O�w���I");
            return;
        }

        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;

        lastManualRestoreTime = Time.time;
        Debug.Log($"[PlayerAttack] ��ʦ^�_ {actualRestored} MP�A��e�]�O: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// �^�_�]�O�]�ѥ~���եΡ^
    /// </summary>
    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;
        Debug.Log($"[PlayerAttack] �^�_ {actualRestored} MP�A��e�]�O: {currentMana}/{maxMana}");
    }

    /// <summary>
    /// �����e�]�O��
    /// </summary>
    public int GetCurrentMana()
    {
        return currentMana;
    }

    /// <summary>
    /// ����]�O�ʤ���
    /// </summary>
    public float GetManaPercentage()
    {
        return (float)currentMana / maxMana;
    }

    /// <summary>
    /// ���a����ˮ`
    /// </summary>
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        Debug.Log($"[PlayerAttack] �D������ {damage} �I�ˮ`�A�Ѿl��q: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// �^�_��q
    /// </summary>
    public void Heal(float healAmount)
    {
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        float actualHealed = currentHealth - oldHealth;
        Debug.Log($"[PlayerAttack] �D���^�_ {actualHealed} ��q�A��e��q: {currentHealth}");
    }

    void Die()
    {
        Debug.Log("[PlayerAttack] �D�����`�I");
    }

    /// <summary>
    /// �]�w�]�O�^�_�}��
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