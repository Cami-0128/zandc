using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 支援TextMeshPro

public class HealthBarUI : MonoBehaviour
{
    [Header("血條UI組件")]
    public Image healthBarFill;        // 血條填充圖片（你的Bar物件）
    public Image healthBarBackground;  // 血條背景圖片（可選）
    public TextMeshProUGUI healthText; // 血量文字（你的Health Text TMP）

    [Header("動畫設定")]
    public float animationSpeed = 2f;      // 血條動畫速度
    public bool smoothTransition = true;   // 是否使用平滑過渡動畫

    [Header("視覺效果")]
    public Color fullHealthColor = Color.green;      // 100-76血量顏色（綠色）
    public Color highHealthColor = Color.yellow;     // 75-51血量顏色（黃色）
    public Color mediumHealthColor = new Color(1f, 0.5f, 0f); // 50-26血量顏色（橘色）
    public Color lowHealthColor = Color.red;         // 25-0血量顏色（紅色）

    [Header("受傷閃爍效果")]
    public bool enableDamageFlash = true;          // 是否啟用受傷閃爍
    public Color damageFlashColor = Color.white;   // 閃爍顏色
    public float flashDuration = 0.1f;             // 閃爍持續時間

    // 私有變數
    private float targetFillAmount;  // 目標填充量
    private float currentFillAmount; // 當前填充量
    private Coroutine flashCoroutine; // 閃爍協程

    void Start()
    {
        // 初始化血條為滿血狀態
        if (healthBarFill != null)
        {
            currentFillAmount = 1f;  // 當前填充量為100%
            targetFillAmount = 1f;   // 目標填充量為100%
            healthBarFill.fillAmount = 1f; // 設置血條為滿血

            // 強制設置初始顏色為白色，然後再設為綠色
            healthBarFill.color = Color.white; // 先設為白色確保能看到顏色變化
            healthBarFill.color = fullHealthColor; // 再設為綠色

            Debug.Log($"血條UI初始化完成，設置顏色為: {fullHealthColor}");
            Debug.Log($"當前血條顏色為: {healthBarFill.color}");
        }
        else
        {
            Debug.LogError("找不到血條填充圖片！請確認Health Bar Fill有正確設置。");
        }
    }

    void Update()
    {
        // 平滑過渡動畫：讓血條慢慢變化而不是瞬間變化
        if (smoothTransition && healthBarFill != null)
        {
            // 如果當前填充量與目標填充量有差距
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                // 慢慢插值到目標值
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
                healthBarFill.fillAmount = currentFillAmount;

                // 確保血量為0時完全隱藏
                if (targetFillAmount <= 0f && currentFillAmount < 0.01f)
                {
                    currentFillAmount = 0f;
                    healthBarFill.fillAmount = 0f;
                }
            }
        }
    }

    // === 主要功能：更新血條 ===
    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        Debug.Log($"HealthBarUI.UpdateHealthBar 被呼叫 - 血量: {currentHealth}/{maxHealth}");

        if (healthBarFill == null)
        {
            Debug.LogError("血條填充圖片未設置！請在Inspector中將Hp拖入Health Bar Fill欄位。");
            return;
        }

        // 計算血量百分比（0.0 到 1.0）
        float healthPercentage = (float)currentHealth / maxHealth;
        Debug.Log($"血量百分比: {healthPercentage:F2} ({healthPercentage:P0})");

        // 設置目標填充量
        targetFillAmount = healthPercentage;
        Debug.Log($"設置目標填充量為: {targetFillAmount}");

        // 如果不使用平滑過渡，直接設置
        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            healthBarFill.fillAmount = currentFillAmount;
            Debug.Log($"直接設置血條填充量為: {currentFillAmount}");
        }
        else
        {
            Debug.Log($"使用平滑過渡，當前填充量: {currentFillAmount} → 目標: {targetFillAmount}");
        }

        // 強制設置血量為0時的填充量
        if (currentHealth <= 0)
        {
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            healthBarFill.fillAmount = 0f;
            Debug.Log("血量歸零，強制設置填充量為0");
        }

        // 更新血條顏色
        UpdateHealthBarColor(healthPercentage);

        // 更新血量文字
        UpdateHealthText(currentHealth, maxHealth);

        // 播放受傷閃爍效果（如果血量下降）
        if (enableDamageFlash && healthPercentage < 1f)
        {
            PlayDamageFlash();
        }
    }

    // === 更新血條顏色（每25點變一次色） ===
    void UpdateHealthBarColor(float healthPercentage)
    {
        if (healthBarFill == null) return;

        Color targetColor;
        string colorName;

        // 根據血量百分比決定顏色（每25%變一次色）
        if (healthPercentage > 0.75f)        // 100-76%：綠色
        {
            targetColor = fullHealthColor;
            colorName = "綠色";
        }
        else if (healthPercentage > 0.5f)    // 75-51%：黃色
        {
            targetColor = highHealthColor;
            colorName = "黃色";
        }
        else if (healthPercentage > 0.25f)   // 50-26%：橘色
        {
            targetColor = mediumHealthColor;
            colorName = "橘色";
        }
        else                                 // 25-0%：紅色
        {
            targetColor = lowHealthColor;
            colorName = "紅色";
        }

        // 強制設置顏色
        healthBarFill.color = targetColor;

        Debug.Log($"血條顏色變更為：{colorName} (血量百分比: {healthPercentage:P0})");
        Debug.Log($"設置的顏色值: R={targetColor.r:F2}, G={targetColor.g:F2}, B={targetColor.b:F2}, A={targetColor.a:F2}");
        Debug.Log($"實際血條顏色: R={healthBarFill.color.r:F2}, G={healthBarFill.color.g:F2}, B={healthBarFill.color.b:F2}, A={healthBarFill.color.a:F2}");
    }

    // === 更新血量文字 ===
    void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    // === 播放受傷閃爍效果 ===
    void PlayDamageFlash()
    {
        // 如果已經在閃爍，先停止舊的閃爍
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // 開始新的閃爍
        if (healthBarFill != null)
        {
            flashCoroutine = StartCoroutine(DamageFlashCoroutine());
            Debug.Log("開始受傷閃爍效果");
        }
    }

    // === 閃爍協程 ===
    IEnumerator DamageFlashCoroutine()
    {
        if (healthBarFill == null)
        {
            Debug.LogWarning("閃爍效果：找不到血條填充圖片");
            yield break;
        }

        // 記住原來的顏色
        Color originalColor = healthBarFill.color;
        Debug.Log($"閃爍效果：原始顏色 {originalColor}，閃爍顏色 {damageFlashColor}");

        // 變成閃爍顏色
        healthBarFill.color = damageFlashColor;

        // 等待閃爍時間
        yield return new WaitForSeconds(flashDuration);

        // 恢復原來的顏色
        healthBarFill.color = originalColor;

        // 重置協程引用
        flashCoroutine = null;
        Debug.Log("閃爍效果結束，恢復原始顏色");
    }

    // === 可選功能：血條震動效果 ===
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
            // 隨機震動
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢復原位置
        transform.localPosition = originalPosition;
    }

    // === 工具函數：設置血條可見性 ===
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // === 工具函數：重置血條到滿血狀態 ===
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

        Debug.Log("血條重置為滿血狀態");
    }
}