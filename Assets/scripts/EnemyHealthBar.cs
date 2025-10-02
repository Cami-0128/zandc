using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("血條UI組件")]
    public Image healthBarFill;
    public Image healthBarBackground;

    [Header("視覺效果 - 血條顏色漸變")]
    [Tooltip("100-76% 血量時的顏色")]
    public Color fullHealthColor = new Color(0f, 1f, 0f);      // 綠色

    [Tooltip("75-51% 血量時的顏色")]
    public Color highHealthColor = new Color(1f, 1f, 0f);      // 黃色

    [Tooltip("50-26% 血量時的顏色")]
    public Color mediumHealthColor = new Color(1f, 0.5f, 0f);  // 橘色

    [Tooltip("25-0% 血量時的顏色")]
    public Color lowHealthColor = new Color(1f, 0f, 0f);       // 紅色

    [Header("位置設定")]
    [Tooltip("血條相對於小怪的位置偏移")]
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Tooltip("血條是否永遠面向攝影機")]
    public bool alwaysFaceCamera = true;

    [Header("動畫設定")]
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
            return fullHealthColor;      // 100-76%: 綠色
        }
        else if (percentage > 0.50f)
        {
            return highHealthColor;      // 75-51%: 黃色
        }
        else if (percentage > 0.25f)
        {
            return mediumHealthColor;    // 50-26%: 橘色
        }
        else
        {
            return lowHealthColor;       // 25-0%: 紅色
        }
    }
}