using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("血條UI組件")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;

    [Header("視覺效果 - 血條顏色漸變")]
    [Tooltip("100-76% 血量時的顏色")]
    [SerializeField] private Color fullHealthColor = new Color(0f, 1f, 0f);      // 綠色

    [Tooltip("75-51% 血量時的顏色")]
    [SerializeField] private Color highHealthColor = new Color(1f, 1f, 0f);      // 黃色

    [Tooltip("50-26% 血量時的顏色")]
    [SerializeField] private Color mediumHealthColor = new Color(1f, 0.5f, 0f);  // 橘色

    [Tooltip("25-0% 血量時的顏色")]
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f);       // 紅色

    [Header("位置設定")]
    [Tooltip("血條相對於小怪的位置偏移")]
    public Vector3 offset = new Vector3(0, 1f, 0);

    [Tooltip("血條是否永遠面向攝影機")]
    public bool alwaysFaceCamera = true;

    [Header("動畫設定")]
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
        // 獲取主攝影機
        mainCamera = Camera.main;

        // 獲取 Canvas 組件
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("[EnemyHealthBar] 缺少 Canvas 組件！");
        }

        // 驗證必要組件
        if (healthBarFill == null)
        {
            Debug.LogError("[EnemyHealthBar] healthBarFill 未設定！", this);
        }

        // 設定初始顏色
        currentColor = fullHealthColor;
        targetColor = fullHealthColor;

        if (healthBarFill != null)
        {
            healthBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// 初始化血條（綁定目標小怪）
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
        // 更新位置
        UpdatePosition();

        // 面向攝影機
        if (alwaysFaceCamera && mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        // 平滑更新血條
        UpdateFillAmount();

        // 平滑更新顏色
        UpdateColor();
    }

    /// <summary>
    /// 更新血條位置（跟隨小怪）
    /// </summary>
    public void UpdatePosition()
    {
        if (targetEnemy != null)
        {
            transform.position = targetEnemy.position + offset;
        }
    }

    /// <summary>
    /// 更新血條顯示
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        // 立即更新或平滑更新
        if (!enableSmoothTransition)
        {
            healthBarFill.fillAmount = targetFillAmount;
        }

        // 更新目標顏色
        targetColor = GetColorForPercentage(healthPercentage);

        if (!enableColorLerp)
        {
            currentColor = targetColor;
            healthBarFill.color = targetColor;
        }
    }

    /// <summary>
    /// 平滑更新填充量
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
    /// 平滑更新顏色
    /// </summary>
    void UpdateColor()
    {
        if (healthBarFill == null || !enableColorLerp) return;

        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
        healthBarFill.color = currentColor;
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
    /// 設定血條偏移位置
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        UpdatePosition();
    }

    /// <summary>
    /// 立即更新血條（無動畫）
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