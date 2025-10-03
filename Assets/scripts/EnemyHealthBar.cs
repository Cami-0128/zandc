using UnityEngine;

/// <summary>
/// 敵人血條系統 - 使用現有的 Sprite 圖片
/// 不動態生成UI，直接使用 Hierarchy 中的圖片
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("血條組件（手動拖入）")]
    [Tooltip("血條填充的 SpriteRenderer（HealthFill 物件）")]
    public SpriteRenderer healthFillSprite;

    [Tooltip("血條外框的 SpriteRenderer（可選，如果要動態顯示/隱藏）")]
    public SpriteRenderer healthBarBackground;

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

    [Header("血條縮放方式")]
    [Tooltip("使用 Scale 還是 Sprite Mask 來控制血量")]
    public bool useScaleMethod = true;

    [Header("動畫設定")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Debug 設定")]
    public bool debugMode = true;

    private Transform enemyTransform;
    private Camera mainCamera;
    private float targetFillAmount;
    private float currentFillAmount;
    private Vector3 originalFillScale;  // 記錄原始縮放

    void Awake()
    {
        mainCamera = Camera.main;
        currentFillAmount = 1f;
        targetFillAmount = 1f;

        // 記錄原始縮放
        if (healthFillSprite != null)
        {
            originalFillScale = healthFillSprite.transform.localScale;
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

        // 設定初始顏色
        if (healthFillSprite != null)
        {
            healthFillSprite.color = fullHealthColor;
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
    /// 更新血條顯示
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillSprite == null)
        {
            Debug.LogWarning("[EnemyHealthBar] healthFillSprite 未設定！");
            return;
        }

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            UpdateFillVisual(targetFillAmount);
        }

        // 更新顏色
        Color targetColor = GetColorForPercentage(healthPercentage);
        healthFillSprite.color = targetColor;

        if (debugMode)
        {
            Debug.Log($"[EnemyHealthBar] 更新血條: {currentHealth}/{maxHealth} ({healthPercentage * 100:F1}%)");
        }
    }

    /// <summary>
    /// 更新血條填充的視覺效果
    /// </summary>
    void UpdateFillVisual(float fillAmount)
    {
        if (healthFillSprite == null) return;

        if (useScaleMethod)
        {
            // 方法1：使用 Scale 縮放（簡單）
            Vector3 newScale = originalFillScale;
            newScale.x = originalFillScale.x * fillAmount;
            healthFillSprite.transform.localScale = newScale;

            // 調整位置，讓血條從左邊開始減少
            float offset = originalFillScale.x * (1f - fillAmount) * 0.5f;
            Vector3 newPos = healthFillSprite.transform.localPosition;
            newPos.x = -offset;
            healthFillSprite.transform.localPosition = newPos;
        }
        else
        {
            // 方法2：使用 Sprite Mask（需要額外設定）
            // 這個方法需要你添加 Sprite Mask 組件
            Debug.LogWarning("[EnemyHealthBar] Sprite Mask 方法需要手動設定 Mask 組件");
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
        if (healthFillSprite != null)
            healthFillSprite.enabled = true;

        if (healthBarBackground != null)
            healthBarBackground.enabled = true;
    }

    /// <summary>
    /// 隱藏血條
    /// </summary>
    public void Hide()
    {
        if (healthFillSprite != null)
            healthFillSprite.enabled = false;

        if (healthBarBackground != null)
            healthBarBackground.enabled = false;
    }
}