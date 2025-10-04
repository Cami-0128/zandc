using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �ĤH����t�� - �ϥ� UI Image ����
/// �����b Hierarchy ���]�w Canvas�BImage �ե�
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("����ե�]��ʩ�J Hierarchy ���� Image�^")]
    [Tooltip("�����R�� Image �ե�]HealthFill�^")]
    public Image healthFillImage;

    [Tooltip("����~�ت� Image �ե�]�i��^")]
    public Image healthBarBackground;

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

    private Transform enemyTransform;
    private Camera mainCamera;
    private float targetFillAmount;
    private float currentFillAmount;

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
    }

    /// <summary>
    /// ��l�Ʀ��
    /// </summary>
    public void Initialize(Transform enemy)
    {
        enemyTransform = enemy;

        if (mainCamera == null)
        {
            Debug.LogError("[EnemyHealthBar] �䤣�� Main Camera�I");
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

        // ���V��v��
        if (alwaysFaceCamera && mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
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

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] ��s���: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
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
}