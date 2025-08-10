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

    void Start()
    {
        // ��l�Ʀ�������媬�A
        if (healthBarFill != null)
        {
            currentFillAmount = 1f;  // ��e��R�q��100%
            targetFillAmount = 1f;   // �ؼж�R�q��100%
            healthBarFill.fillAmount = 1f; // �]�m���������

            // �j��]�m��l�C�⬰�զ�A�M��A�]�����
            healthBarFill.color = Color.white; // ���]���զ�T�O��ݨ��C���ܤ�
            healthBarFill.color = fullHealthColor; // �A�]�����

            Debug.Log($"���UI��l�Ƨ����A�]�m�C�⬰: {fullHealthColor}");
            Debug.Log($"��e����C�⬰: {healthBarFill.color}");
        }
        else
        {
            Debug.LogError("�䤣������R�Ϥ��I�нT�{Health Bar Fill�����T�]�m�C");
        }
    }

    void Update()
    {
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

        // ������˰{�{�ĪG�]�p�G��q�U���^
        if (enableDamageFlash && healthPercentage < 1f)
        {
            PlayDamageFlash();
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
        Debug.Log($"�]�m���C���: R={targetColor.r:F2}, G={targetColor.g:F2}, B={targetColor.b:F2}, A={targetColor.a:F2}");
        Debug.Log($"��ڦ���C��: R={healthBarFill.color.r:F2}, G={healthBarFill.color.g:F2}, B={healthBarFill.color.b:F2}, A={healthBarFill.color.a:F2}");
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

    // === �u���ơG���m����캡�媬�A ===
    public void ResetHealthBar()
    {
        if (healthBarFill != null)
        {
            currentFillAmount = 1f;
            targetFillAmount = 1f;
            healthBarFill.fillAmount = 1f;
            UpdateHealthBarColor(1f);
        }

        if (healthText != null)
        {
            PlayerController2D player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                int maxHealth = player.GetMaxHealth();
                healthText.text = $"{maxHealth}/{maxHealth}";
            }
        }

        Debug.Log("������m�����媬�A");
    }
}