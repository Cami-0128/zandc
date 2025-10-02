using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("���UI�ե�")]
    public Image healthBarFill;
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
    [Tooltip("����۹��p�Ǫ���m����")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Tooltip("����O�_�û����V��v��")]
    public bool alwaysFaceCamera = true;

    [Header("�ʵe�]�w")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;

    private Transform targetEnemy;
    private Camera mainCamera;
    private float targetFillAmount;
    private float currentFillAmount;

    void Awake()
    {
        mainCamera = Camera.main;
        currentFillAmount = 1f;
        targetFillAmount = 1f;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            healthBarFill.color = fullHealthColor;
        }
    }

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
        UpdatePosition();

        if (alwaysFaceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        if (enableSmoothTransition && healthBarFill != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
                healthBarFill.fillAmount = currentFillAmount;
            }
        }
    }

    public void UpdatePosition()
    {
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + offset;
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = targetFillAmount;
        }

        Color targetColor = GetColorForPercentage(healthPercentage);
        healthBarFill.color = targetColor;
    }

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
}