using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敵人血條系統 - 使用 UI Image 版本
/// 直接在 Hierarchy 中設定 Canvas、Image 組件
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("血條組件（手動拖入 Hierarchy 中的 Image）")]
    [Tooltip("血條填充的 Image 組件（HealthFill）")]
    public Image healthFillImage;

    [Tooltip("血條外框的 Image 組件（可選）")]
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
    [Tooltip("血條相對於敵人的局部位置")]
    public Vector3 localOffset = new Vector3(0, 1.5f, 0);

    [Tooltip("血條是否永遠面向攝影機")]
    public bool alwaysFaceCamera = true;

    [Header("動畫設定")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Debug 設定")]
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

        // 設定 Image 為 Filled 模式
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
    /// 初始化血條
    /// </summary>
    public void Initialize(Transform enemy)
    {
        enemyTransform = enemy;

        if (mainCamera == null)
        {
            Debug.LogError("[EnemyHealthBar] 找不到 Main Camera！");
        }

        // 設定局部位置
        transform.localPosition = localOffset;

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] 血條初始化完成");
        }
    }

    void LateUpdate()
    {
        if (enemyTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // 面向攝影機
        if (alwaysFaceCamera && mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }

        // 平滑過渡動畫
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
    /// 更新血條顯示
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage == null)
        {
            Debug.LogWarning("[EnemyHealthBar] healthFillImage 未設定！");
            return;
        }

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            healthFillImage.fillAmount = targetFillAmount;
        }

        // 更新顏色
        Color targetColor = GetColorForPercentage(healthPercentage);
        healthFillImage.color = targetColor;

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] 更新血條: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
    }

    /// <summary>
    /// 根據血量百分比獲取對應顏色
    /// </summary>
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

    /// <summary>
    /// 顯示血條
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏血條
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}