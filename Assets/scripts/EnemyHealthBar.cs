using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("���UI�ե�")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;

    [Header("��ı�ĪG - ����C�⺥��")]
    [Tooltip("100-76% ��q�ɪ��C��")]
    [SerializeField] private Color fullHealthColor = new Color(0f, 1f, 0f);      // ���

    [Tooltip("75-51% ��q�ɪ��C��")]
    [SerializeField] private Color highHealthColor = new Color(1f, 1f, 0f);      // ����

    [Tooltip("50-26% ��q�ɪ��C��")]
    [SerializeField] private Color mediumHealthColor = new Color(1f, 0.5f, 0f);  // ���

    [Tooltip("25-0% ��q�ɪ��C��")]
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f);       // ����

    [Header("��m�]�w")]
    [Tooltip("����۹��p�Ǫ���m����")]
    public Vector3 offset = new Vector3(0, 1f, 0);

    [Tooltip("����O�_�û����V��v��")]
    public bool alwaysFaceCamera = true;

    [Header("�ʵe�]�w")]
    [SerializeField] private bool enableSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private bool enableColorLerp = true;
    [SerializeField] private float colorTransitionSpeed = 3f;

    private Transform targetEnemy;
    private Camera mainCamera;
    private float targetFillAmount;
    private Color currentColor;
    private Color targetColor;
    private Canvas canvas;

    void Awake()
    {
        // ����D��v��
        mainCamera = Camera.main;

        // ��� Canvas �ե�
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("[EnemyHealthBar] �ʤ� Canvas �ե�I");
        }

        // ���ҥ��n�ե�
        if (healthBarFill == null)
        {
            Debug.LogError("[EnemyHealthBar] healthBarFill ���]�w�I", this);
        }

        // �]�w��l�C��
        currentColor = fullHealthColor;
        targetColor = fullHealthColor;

        if (healthBarFill != null)
        {
            healthBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// ��l�Ʀ���]�j�w�ؼФp�ǡ^
    /// </summary>
    public void Initialize(Transform enemy)
    {
        targetEnemy = enemy;

        if (healthBarFill != null)
        {
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthBarFill.fillAmount = 1f;
        }

        UpdatePosition();
    }

    void LateUpdate()
    {
        // ��s��m
        UpdatePosition();

        // ���V��v��
        if (alwaysFaceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        // ���Ƨ�s���
        UpdateFillAmount();

        // ���Ƨ�s�C��
        UpdateColor();
    }

    /// <summary>
    /// ��s�����m�]���H�p�ǡ^
    /// </summary>
    public void UpdatePosition()
    {
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + offset;
        }
    }

    /// <summary>
    /// ��s������
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        // �ߧY��s�Υ��Ƨ�s
        if (!enableSmoothTransition)
        {
            healthBarFill.fillAmount = targetFillAmount;
        }

        // ��s�ؼ��C��
        targetColor = GetColorForPercentage(healthPercentage);

        if (!enableColorLerp)
        {
            currentColor = targetColor;
            healthBarFill.color = targetColor;
        }
    }

    /// <summary>
    /// ���Ƨ�s��R�q
    /// </summary>
    void UpdateFillAmount()
    {
        if (healthBarFill == null || !enableSmoothTransition) return;

        healthBarFill.fillAmount = Mathf.Lerp(
            healthBarFill.fillAmount,
            targetFillAmount,
            Time.deltaTime * transitionSpeed
        );
    }

    /// <summary>
    /// ���Ƨ�s�C��
    /// </summary>
    void UpdateColor()
    {
        if (healthBarFill == null || !enableColorLerp) return;

        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
        healthBarFill.color = currentColor;
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
    /// �]�w���������m
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        UpdatePosition();
    }

    /// <summary>
    /// �ߧY��s����]�L�ʵe�^
    /// </summary>
    public void UpdateHealthBarImmediate(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);

        healthBarFill.fillAmount = healthPercentage;
        targetFillAmount = healthPercentage;

        Color newColor = GetColorForPercentage(healthPercentage);
        healthBarFill.color = newColor;
        currentColor = newColor;
        targetColor = newColor;
    }
}