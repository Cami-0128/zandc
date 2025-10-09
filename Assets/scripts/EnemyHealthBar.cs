using UnityEngine;
using UnityEngine.UI;
using TMPro; // �ޤJ TextMeshPro

/// <summary>
/// �ĤH����t�� - �ϥ� UI Image + TextMeshPro ��ܦ�q�Ʀr
/// �����b Hierarchy ���]�w Canvas�BImage�BText �ե�
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("����ե�]��ʩ�J Hierarchy ���� Image�^")]
    [Tooltip("�����R�� Image �ե�]HealthFill�^")]
    public Image healthFillImage;

    [Tooltip("����~�ت� Image �ե�]�i��^")]
    public Image healthBarBackground;

    [Header("��q��r���")]
    [Tooltip("��ܦ�q�Ʀr�� TextMeshProUGUI �ե�")]
    public TextMeshProUGUI healthText;

    [Tooltip("��r��ܮ榡")]
    public HealthTextFormat textFormat = HealthTextFormat.CurrentAndMax;

    [Tooltip("�O�_��ܦ�q��r")]
    public bool showHealthText = true;

    [Header("��ı�ĪG - ����C�⺥��")]
    [Tooltip("100-76% ��q�ɪ��C��")]
    public Color fullHealthColor = new Color(0f, 1f, 0f);      // ���
    [Tooltip("75-51% ��q�ɪ��C��")]
    public Color highHealthColor = new Color(1f, 1f, 0f);      // ����
    [Tooltip("50-26% ��q�ɪ��C��")]
    public Color mediumHealthColor = new Color(1f, 0.5f, 0f);  // ���
    [Tooltip("25-0% ��q�ɪ��C��")]
    public Color lowHealthColor = new Color(1f, 0f, 0f);       // ����

    [Header("��m�]�w")]
    [Tooltip("����۹��ĤH��������m")]
    public Vector3 localOffset = new Vector3(0, 1.5f, 0);

    [Tooltip("����O�_�û����V��v��")]
    public bool alwaysFaceCamera = true;

    [Header("�ʵe�]�w")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Debug �]�w")]
    public bool debugMode = false;

    // �p���ܼ�
    private Transform enemyTransform;
    private Camera mainCamera;
    private float targetFillAmount;
    private float currentFillAmount;
    private float currentHealthValue;
    private float maxHealthValue;

    /// <summary>
    /// ��q��r��ܮ榡
    /// </summary>
    public enum HealthTextFormat
    {
        CurrentOnly,        // �u��ܷ�e��q: "85"
        CurrentAndMax,      // ��ܷ�e/�̤j: "85/100"
        Percentage,         // ��ܦʤ���: "85%"
        CurrentAndPercent   // ��ܷ�e+�ʤ���: "85 (85%)"
    }

    void Awake()
    {
        mainCamera = Camera.main;
        currentFillAmount = 1f;
        targetFillAmount = 1f;

        // �]�w Image �� Filled �Ҧ�
        if (healthFillImage != null)
        {
            healthFillImage.type = Image.Type.Filled;
            healthFillImage.fillMethod = Image.FillMethod.Horizontal;
            healthFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthFillImage.fillAmount = 1f;
            healthFillImage.color = fullHealthColor;
        }

        // ��l�Ƥ�r�ե�
        if (healthText != null)
        {
            healthText.gameObject.SetActive(showHealthText);
        }
    }

    /// <summary>
    /// ��l�Ʀ��
    /// </summary>
    public void Initialize(Transform enemy)
    {
        enemyTransform = enemy;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[EnemyHealthBar] �䤣�� Main Camera�I");
            }
        }

        // �]�w������m
        transform.localPosition = localOffset;

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] �����l�Ƨ���");
        }
    }

    void LateUpdate()
    {
        if (enemyTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // ���V��v���]�ץ��ϦV���D�^
        if (alwaysFaceCamera && mainCamera != null)
        {
            // ��k1: �����ϥ���v��������
            transform.rotation = mainCamera.transform.rotation;

            // �Τ�k2: �ϥ� LookRotation ���ϦV
            // Vector3 directionToCamera = transform.position - mainCamera.transform.position;
            // transform.rotation = Quaternion.LookRotation(directionToCamera);
        }

        // ���ƹL��ʵe
        if (enableSmoothTransition && healthFillImage != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
                healthFillImage.fillAmount = currentFillAmount;
            }
            else
            {
                currentFillAmount = targetFillAmount;
                healthFillImage.fillAmount = targetFillAmount;
            }
        }
    }

    /// <summary>
    /// ��s������
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage == null)
        {
            Debug.LogWarning("[EnemyHealthBar] healthFillImage ���]�w�I");
            return;
        }

        // �x�s��q�ȨѤ�r��s�ϥ�
        currentHealthValue = currentHealth;
        maxHealthValue = maxHealth;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            healthFillImage.fillAmount = targetFillAmount;
        }

        // ��s�C��
        Color targetColor = GetColorForPercentage(healthPercentage);
        healthFillImage.color = targetColor;

        // ��s��q��r
        UpdateHealthText(currentHealth, maxHealth, healthPercentage);

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] ��s���: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
    }

    /// <summary>
    /// ��s��q��r���
    /// </summary>
    private void UpdateHealthText(float currentHealth, float maxHealth, float percentage)
    {
        if (healthText == null || !showHealthText) return;

        string displayText = "";

        switch (textFormat)
        {
            case HealthTextFormat.CurrentOnly:
                // �u��ܷ�e��q: "85"
                displayText = Mathf.CeilToInt(currentHealth).ToString();
                break;

            case HealthTextFormat.CurrentAndMax:
                // ��ܷ�e/�̤j: "85/100"
                displayText = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
                break;

            case HealthTextFormat.Percentage:
                // ��ܦʤ���: "85%"
                displayText = $"{Mathf.RoundToInt(percentage * 100)}%";
                break;

            case HealthTextFormat.CurrentAndPercent:
                // ��ܷ�e+�ʤ���: "85 (85%)"
                displayText = $"{Mathf.CeilToInt(currentHealth)} ({Mathf.RoundToInt(percentage * 100)}%)";
                break;
        }

        healthText.text = displayText;
    }

    /// <summary>
    /// �ھڦ�q�ʤ�����������C��
    /// </summary>
    Color GetColorForPercentage(float percentage)
    {
        if (percentage > 0.75f)
        {
            return fullHealthColor;      // 100-76%: ���
        }
        else if (percentage > 0.50f)
        {
            return highHealthColor;      // 75-51%: ����
        }
        else if (percentage > 0.25f)
        {
            return mediumHealthColor;    // 50-26%: ���
        }
        else
        {
            return lowHealthColor;       // 25-0%: ����
        }
    }

    /// <summary>
    /// ��ܦ��
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// ���æ��
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// �]�w�O�_��ܦ�q��r
    /// </summary>
    public void SetShowHealthText(bool show)
    {
        showHealthText = show;
        if (healthText != null)
        {
            healthText.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// �]�w��r��ܮ榡
    /// </summary>
    public void SetTextFormat(HealthTextFormat format)
    {
        textFormat = format;
        // �ߧY��s��r���
        if (maxHealthValue > 0)
        {
            UpdateHealthText(currentHealthValue, maxHealthValue, currentHealthValue / maxHealthValue);
        }
    }
}