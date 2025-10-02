using System.Collections;
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

    [Header("�]�O�t��")]
    [Tooltip("�C���������Ӫ��]�O")]
    public int attackManaCost = 5;
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
        // === ����G�ˬd���a�O�_���` ===
        if (playerController != null && playerController.isDead)
        {
            return; // ���`�᪽����^�A���������ާ@
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

    void CastMagic()
    {
        // �A���ˬd�O�_���`�]�����O�I�^
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (currentMana < attackManaCost)
        {
            Debug.Log("�]�O�����I��e�]�O: " + currentMana + "/" + maxMana);
            return;
        }

        if (magicBulletPrefab == null)
        {
            Debug.LogError("���]�w�]�k�l�uPrefab�I");
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
        Debug.Log("���� " + amount + " MP�A�Ѿl: " + currentMana + "/" + maxMana);
    }

    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            // === ���`�ɰ���^�] ===
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
        // ���`�ɵL�k��ʦ^�]
        if (playerController != null && playerController.isDead)
        {
            return;
        }

        if (Time.time - lastManualRestoreTime < manualRestoreCooldown)
        {
            float remainingCooldown = manualRestoreCooldown - (Time.time - lastManualRestoreTime);
            Debug.Log("��ʦ^�]�N�o���I�Ѿl�ɶ�: " + remainingCooldown.ToString("F1") + " ��");
            return;
        }

        if (currentMana >= maxMana)
        {
            Debug.Log("�]�O�w���I");
            return;
        }

        int oldMana = currentMana;
        currentMana += manualRestoreAmount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;

        lastManualRestoreTime = Time.time;
        Debug.Log("��ʦ^�_ " + actualRestored + " MP�A��e�]�O: " + currentMana + "/" + maxMana);
    }

    public void RestoreMana(int amount)
    {
        int oldMana = currentMana;
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        int actualRestored = currentMana - oldMana;
        Debug.Log("�^�_ " + actualRestored + " MP�A��e�]�O: " + currentMana + "/" + maxMana);
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
        Debug.Log("�D������ " + damage + " �I�ˮ`�A�Ѿl��q: " + currentHealth);

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
        Debug.Log("�D���^�_ " + actualHealed + " ��q�A��e��q: " + currentHealth);
    }

    void Die()
    {
        Debug.Log("�D�����`�I");
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