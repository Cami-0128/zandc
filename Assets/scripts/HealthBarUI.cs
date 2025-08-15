using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // �䴩TextMeshPro

public class HealthBarUI : MonoBehaviour
{
    [Header("���UI�ե�")]
    public Image healthBarFill;        // �����R�Ϥ��]�A��Bar����^
    public Image healthBarBackground;  // ����I���Ϥ��]�i��^
    public TextMeshProUGUI healthText; // ��q��r�]�A��Health Text TMP�^

    [Header("�ʵe�]�w")]
    public float animationSpeed = 2f;      // ����ʵe�t��
    public bool smoothTransition = true;   // �O�_�ϥΥ��ƹL��ʵe

    [Header("��ı�ĪG")]
    public Color fullHealthColor = Color.green;      // 100-76��q�C��]���^
    public Color highHealthColor = Color.yellow;     // 75-51��q�C��]����^
    public Color mediumHealthColor = new Color(1f, 0.5f, 0f); // 50-26��q�C��]���^
    public Color lowHealthColor = Color.red;         // 25-0��q�C��]����^

    [Header("���˰{�{�ĪG")]
    public bool enableDamageFlash = true;          // �O�_�ҥΨ��˰{�{
    public Color damageFlashColor = Color.white;   // �{�{�C��
    public float flashDuration = 0.1f;             // �{�{����ɶ�

    // �p���ܼ�
    private float targetFillAmount;  // �ؼж�R�q
    private float currentFillAmount; // ��e��R�q
    private Coroutine flashCoroutine; // �{�{��{
    private int lastKnownHealth = -1; // �O���W������q�]�Ω��˴��O�_���ˡ^
    private bool isInitialized = false; // �O�_�w�g��l��

    void Start()
    {
        // === �ק�G�����l�ơA���ݪ��a���J ===
        StartCoroutine(InitializeHealthBar());
    }

    // === �s�W�G��l�ƨ�{ ===
    IEnumerator InitializeHealthBar()
    {
        // ���ݤ@�V�A�T�O���a����w�g��l��
        yield return new WaitForEndOfFrame();

        // �M�䪱�a���
        PlayerController2D player = FindObjectOfType<PlayerController2D>();

        // ���ݪ��a���J
        int attempts = 0;
        while (player == null && attempts < 100) // �̦h����100�V
        {
            yield return new WaitForEndOfFrame();
            player = FindObjectOfType<PlayerController2D>();
            attempts++;
        }

        if (player != null)
        {
            // === �ק�G�ھڪ��a����ڦ�q��l�Ʀ�� ===
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[�����l��] ���J���a��q: {currentHealth}/{maxHealth}");

            // �p���q�ʤ���
            float healthPercentage = (float)currentHealth / maxHealth;

            // �]�m���
            currentFillAmount = healthPercentage;
            targetFillAmount = healthPercentage;

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercentage;
                UpdateHealthBarColor(healthPercentage);
                Debug.Log($"�����l�Ƨ��� - ��q: {currentHealth}/{maxHealth} ({healthPercentage:P0})");
            }

            // ��s��q��r
            UpdateHealthText(currentHealth, maxHealth);

            // �O����l��q
            lastKnownHealth = currentHealth;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("�䤣�쪱�a����I����L�k���T��l�ơC");

            // === �ƥΤ�סG�]������ ===
            if (healthBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                healthBarFill.fillAmount = 1f;
                healthBarFill.color = fullHealthColor;
                Debug.Log("�ϥγƥΪ�l�Ƥ�סA�]�m���������");
            }
        }
    }

    void Update()
    {
        // === �s�W�G�۰ʦP�B��q�]����P�B�^ ===
        if (isInitialized)
        {
            AutoSyncWithPlayer();
        }

        // ���ƹL��ʵe�G������C�C�ܤƦӤ��O�����ܤ�
        if (smoothTransition && healthBarFill != null)
        {
            // �p�G��e��R�q�P�ؼж�R�q���t�Z
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                // �C�C���Ȩ�ؼЭ�
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
                healthBarFill.fillAmount = currentFillAmount;

                // �T�O��q��0�ɧ�������
                if (targetFillAmount <= 0f && currentFillAmount < 0.01f)
                {
                    currentFillAmount = 0f;
                    healthBarFill.fillAmount = 0f;
                }
            }
        }
    }

    // === �s�W�G�۰ʦP�B���a��q ===
    void AutoSyncWithPlayer()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();
        if (player != null)
        {
            int currentHealth = player.GetCurrentHealth();

            // �p�G��q���ܤơA�۰ʧ�s���
            if (currentHealth != lastKnownHealth)
            {
                int maxHealth = player.GetMaxHealth();
                Debug.Log($"[�۰ʦP�B] �˴����q�ܤ�: {lastKnownHealth} �� {currentHealth}");
                UpdateHealthBar(currentHealth, maxHealth);
                lastKnownHealth = currentHealth;
            }
        }
    }

    // === �D�n�\��G��s��� ===
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        Debug.Log($"HealthBarUI.UpdateHealthBar �Q�I�s - ��q: {currentHealth}/{maxHealth}");

        if (healthBarFill == null)
        {
            Debug.LogError("�����R�Ϥ����]�m�I�ЦbInspector���NHp��JHealth Bar Fill���C");
            return;
        }

        // �p���q�ʤ���]0.0 �� 1.0�^
        float healthPercentage = (float)currentHealth / maxHealth;
        Debug.Log($"��q�ʤ���: {healthPercentage:F2} ({healthPercentage:P0})");

        // === �s�W�G�˴��O�_���ˡ]�Ω�{�{�ĪG�^ ===
        bool tookDamage = (lastKnownHealth > 0 && currentHealth < lastKnownHealth);

        // �]�m�ؼж�R�q
        targetFillAmount = healthPercentage;
        Debug.Log($"�]�m�ؼж�R�q��: {targetFillAmount}");

        // �p�G���ϥΥ��ƹL��A�����]�m
        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = currentFillAmount;
            Debug.Log($"�����]�m�����R�q��: {currentFillAmount}");
        }
        else
        {
            Debug.Log($"�ϥΥ��ƹL��A��e��R�q: {currentFillAmount} �� �ؼ�: {targetFillAmount}");
        }

        // �j��]�m��q��0�ɪ���R�q
        if (currentHealth <= 0)
        {
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            healthBarFill.fillAmount = 0f;
            Debug.Log("��q�k�s�A�j��]�m��R�q��0");
        }

        // ��s����C��
        UpdateHealthBarColor(healthPercentage);

        // ��s��q��r
        UpdateHealthText(currentHealth, maxHealth);

        // === �ק�G�u�����ˮɤ~����{�{�ĪG ===
        if (enableDamageFlash && tookDamage)
        {
            PlayDamageFlash();
            Debug.Log($"�˴�����ˡA����{�{�ĪG ({lastKnownHealth} �� {currentHealth})");
        }

        // ��s�O������q
        lastKnownHealth = currentHealth;

        // �аO���w��l��
        if (!isInitialized)
        {
            isInitialized = true;
        }
    }

    // === ��s����C��]�C25�I�ܤ@����^ ===
    void UpdateHealthBarColor(float healthPercentage)
    {
        if (healthBarFill == null) return;

        Color targetColor;
        string colorName;

        // �ھڦ�q�ʤ���M�w�C��]�C25%�ܤ@����^
        if (healthPercentage > 0.75f)        // 100-76%�G���
        {
            targetColor = fullHealthColor;
            colorName = "���";
        }
        else if (healthPercentage > 0.5f)    // 75-51%�G����
        {
            targetColor = highHealthColor;
            colorName = "����";
        }
        else if (healthPercentage > 0.25f)   // 50-26%�G���
        {
            targetColor = mediumHealthColor;
            colorName = "���";
        }
        else                                 // 25-0%�G����
        {
            targetColor = lowHealthColor;
            colorName = "����";
        }

        // �j��]�m�C��
        healthBarFill.color = targetColor;

        Debug.Log($"����C���ܧ󬰡G{colorName} (��q�ʤ���: {healthPercentage:P0})");
    }

    // === ��s��q��r ===
    void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    // === ������˰{�{�ĪG ===
    void PlayDamageFlash()
    {
        // �p�G�w�g�b�{�{�A�������ª��{�{
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // �}�l�s���{�{
        if (healthBarFill != null)
        {
            flashCoroutine = StartCoroutine(DamageFlashCoroutine());
            Debug.Log("�}�l���˰{�{�ĪG");
        }
    }

    // === �{�{��{ ===
    IEnumerator DamageFlashCoroutine()
    {
        if (healthBarFill == null)
        {
            Debug.LogWarning("�{�{�ĪG�G�䤣������R�Ϥ�");
            yield break;
        }

        // �O���Ӫ��C��
        Color originalColor = healthBarFill.color;
        Debug.Log($"�{�{�ĪG�G��l�C�� {originalColor}�A�{�{�C�� {damageFlashColor}");

        // �ܦ��{�{�C��
        healthBarFill.color = damageFlashColor;

        // ���ݰ{�{�ɶ�
        yield return new WaitForSeconds(flashDuration);

        // ��_��Ӫ��C��
        healthBarFill.color = originalColor;

        // ���m��{�ޥ�
        flashCoroutine = null;
        Debug.Log("�{�{�ĪG�����A��_��l�C��");
    }

    // === �i��\��G����_�ʮĪG ===
    public void ShakeHealthBar(float intensity = 1f, float duration = 0.2f)
    {
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // �H���_��
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ��_���m
        transform.localPosition = originalPosition;
    }

    // === �u���ơG�]�m����i���� ===
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // === �ק�G������m�]�t�X�����d��q�t�Ρ^ ===
    public void ResetHealthBar()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();

        if (player != null)
        {
            // === �ϥΪ��a����ڦ�q�A�Ӥ��O�j��� ===
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[������m] ���m�����a��e��q: {currentHealth}/{maxHealth}");
            UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            // === �ƥΤ�סG�]������ ===
            if (healthBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                healthBarFill.fillAmount = 1f;
                UpdateHealthBarColor(1f);
            }

            if (healthText != null)
            {
                healthText.text = "100/100"; // �w�]��
            }

            Debug.Log("������m���w�]���媬�A�]�䤣�쪱�a�^");
        }

        // ���m���A
        lastKnownHealth = -1;
        isInitialized = false;

        // ���s��l��
        StartCoroutine(InitializeHealthBar());
    }

    // === �s�W�G�j��P�B��q�]�ѥ~���I�s�^ ===
    public void ForceSync()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();
        if (player != null)
        {
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[�j��P�B] �P�B��q: {currentHealth}/{maxHealth}");
            UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    // === �s�W�G���������A�]�����Ρ^ ===
    public void LogHealthBarStatus()
    {
        Debug.Log($"[������A] �w��l��: {isInitialized}");
        Debug.Log($"[������A] �W����q: {lastKnownHealth}");
        Debug.Log($"[������A] ��e��R: {currentFillAmount:F2}");
        Debug.Log($"[������A] �ؼж�R: {targetFillAmount:F2}");

        if (healthBarFill != null)
        {
            Debug.Log($"[������A] ��ڶ�R: {healthBarFill.fillAmount:F2}");
            Debug.Log($"[������A] ����C��: {healthBarFill.color}");
        }
    }
}