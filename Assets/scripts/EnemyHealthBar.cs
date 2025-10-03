using UnityEngine;

/// <summary>
/// �ĤH����t�� - �ϥβ{���� Sprite �Ϥ�
/// ���ʺA�ͦ�UI�A�����ϥ� Hierarchy �����Ϥ�
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("����ե�]��ʩ�J�^")]
    [Tooltip("�����R�� SpriteRenderer�]HealthFill ����^")]
    public SpriteRenderer healthFillSprite;

    [Tooltip("����~�ت� SpriteRenderer�]�i��A�p�G�n�ʺA���/���á^")]
    public SpriteRenderer healthBarBackground;

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

    [Header("����Y��覡")]
    [Tooltip("�ϥ� Scale �٬O Sprite Mask �ӱ����q")]
    public bool useScaleMethod = true;

    [Header("�ʵe�]�w")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Debug �]�w")]
    public bool debugMode = true;

    private Transform enemyTransform;
    private Camera mainCamera;
    private float targetFillAmount;
    private float currentFillAmount;
    private Vector3 originalFillScale;  // �O����l�Y��

    void Awake()
    {
        mainCamera = Camera.main;
        currentFillAmount = 1f;
        targetFillAmount = 1f;

        // �O����l�Y��
        if (healthFillSprite != null)
        {
            originalFillScale = healthFillSprite.transform.localScale;
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

        // �]�w��l�C��
        if (healthFillSprite != null)
        {
            healthFillSprite.color = fullHealthColor;
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
        if (enableSmoothTransition)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
                UpdateFillVisual(currentFillAmount);
            }
        }
    }

    /// <summary>
    /// ��s������
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillSprite == null)
        {
            Debug.LogWarning("[EnemyHealthBar] healthFillSprite ���]�w�I");
            return;
        }

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            UpdateFillVisual(targetFillAmount);
        }

        // ��s�C��
        Color targetColor = GetColorForPercentage(healthPercentage);
        healthFillSprite.color = targetColor;

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] ��s���: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
    }

    /// <summary>
    /// ��s�����R����ı�ĪG
    /// </summary>
    void UpdateFillVisual(float fillAmount)
    {
        if (healthFillSprite == null) return;

        if (useScaleMethod)
        {
            // ��k1�G�ϥ� Scale �Y��]²��^
            Vector3 newScale = originalFillScale;
            newScale.x = originalFillScale.x * fillAmount;
            healthFillSprite.transform.localScale = newScale;

            // �վ��m�A������q����}�l���
            float offset = originalFillScale.x * (1f - fillAmount) * 0.5f;
            Vector3 newPos = healthFillSprite.transform.localPosition;
            newPos.x = -offset;
            healthFillSprite.transform.localPosition = newPos;
        }
        else
        {
            // ��k2�G�ϥ� Sprite Mask�]�ݭn�B�~�]�w�^
            // �o�Ӥ�k�ݭn�A�K�[ Sprite Mask �ե�
            Debug.LogWarning("[EnemyHealthBar] Sprite Mask ��k�ݭn��ʳ]�w Mask �ե�");
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
        if (healthFillSprite != null)
            healthFillSprite.enabled = true;

        if (healthBarBackground != null)
            healthBarBackground.enabled = true;
    }

    /// <summary>
    /// ���æ��
    /// </summary>
    public void Hide()
    {
        if (healthFillSprite != null)
            healthFillSprite.enabled = false;

        if (healthBarBackground != null)
            healthBarBackground.enabled = false;
    }
}