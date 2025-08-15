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
    private int lastKnownHealth = -1; // 記錄上次的血量（用於檢測是否受傷）
    private bool isInitialized = false; // 是否已經初始化

    void Start()
    {
        // === 修改：延遲初始化，等待玩家載入 ===
        StartCoroutine(InitializeHealthBar());
    }

    // === 新增：初始化協程 ===
    IEnumerator InitializeHealthBar()
    {
        // 等待一幀，確保玩家控制器已經初始化
        yield return new WaitForEndOfFrame();

        // 尋找玩家控制器
        PlayerController2D player = FindObjectOfType<PlayerController2D>();

        // 等待玩家載入
        int attempts = 0;
        while (player == null && attempts < 100) // 最多等待100幀
        {
            yield return new WaitForEndOfFrame();
            player = FindObjectOfType<PlayerController2D>();
            attempts++;
        }

        if (player != null)
        {
            // === 修改：根據玩家的實際血量初始化血條 ===
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[血條初始化] 載入玩家血量: {currentHealth}/{maxHealth}");

            // 計算血量百分比
            float healthPercentage = (float)currentHealth / maxHealth;

            // 設置血條
            currentFillAmount = healthPercentage;
            targetFillAmount = healthPercentage;

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercentage;
                UpdateHealthBarColor(healthPercentage);
                Debug.Log($"血條初始化完成 - 血量: {currentHealth}/{maxHealth} ({healthPercentage:P0})");
            }

            // 更新血量文字
            UpdateHealthText(currentHealth, maxHealth);

            // 記錄初始血量
            lastKnownHealth = currentHealth;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("找不到玩家控制器！血條無法正確初始化。");

            // === 備用方案：設為滿血 ===
            if (healthBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                healthBarFill.fillAmount = 1f;
                healthBarFill.color = fullHealthColor;
                Debug.Log("使用備用初始化方案，設置血條為滿血");
            }
        }
    }

    void Update()
    {
        // === 新增：自動同步血量（防止不同步） ===
        if (isInitialized)
        {
            AutoSyncWithPlayer();
        }

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

    // === 新增：自動同步玩家血量 ===
    void AutoSyncWithPlayer()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();
        if (player != null)
        {
            int currentHealth = player.GetCurrentHealth();

            // 如果血量有變化，自動更新血條
            if (currentHealth != lastKnownHealth)
            {
                int maxHealth = player.GetMaxHealth();
                Debug.Log($"[自動同步] 檢測到血量變化: {lastKnownHealth} → {currentHealth}");
                UpdateHealthBar(currentHealth, maxHealth);
                lastKnownHealth = currentHealth;
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

        // === 新增：檢測是否受傷（用於閃爍效果） ===
        bool tookDamage = (lastKnownHealth > 0 && currentHealth < lastKnownHealth);

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

        // === 修改：只有受傷時才播放閃爍效果 ===
        if (enableDamageFlash && tookDamage)
        {
            PlayDamageFlash();
            Debug.Log($"檢測到受傷，播放閃爍效果 ({lastKnownHealth} → {currentHealth})");
        }

        // 更新記錄的血量
        lastKnownHealth = currentHealth;

        // 標記為已初始化
        if (!isInitialized)
        {
            isInitialized = true;
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

    // === 修改：血條重置（配合跨關卡血量系統） ===
    public void ResetHealthBar()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();

        if (player != null)
        {
            // === 使用玩家的實際血量，而不是強制滿血 ===
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[血條重置] 重置為玩家當前血量: {currentHealth}/{maxHealth}");
            UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            // === 備用方案：設為滿血 ===
            if (healthBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                healthBarFill.fillAmount = 1f;
                UpdateHealthBarColor(1f);
            }

            if (healthText != null)
            {
                healthText.text = "100/100"; // 預設值
            }

            Debug.Log("血條重置為預設滿血狀態（找不到玩家）");
        }

        // 重置狀態
        lastKnownHealth = -1;
        isInitialized = false;

        // 重新初始化
        StartCoroutine(InitializeHealthBar());
    }

    // === 新增：強制同步血量（供外部呼叫） ===
    public void ForceSync()
    {
        PlayerController2D player = FindObjectOfType<PlayerController2D>();
        if (player != null)
        {
            int currentHealth = player.GetCurrentHealth();
            int maxHealth = player.GetMaxHealth();

            Debug.Log($"[強制同步] 同步血量: {currentHealth}/{maxHealth}");
            UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    // === 新增：獲取血條狀態（除錯用） ===
    public void LogHealthBarStatus()
    {
        Debug.Log($"[血條狀態] 已初始化: {isInitialized}");
        Debug.Log($"[血條狀態] 上次血量: {lastKnownHealth}");
        Debug.Log($"[血條狀態] 當前填充: {currentFillAmount:F2}");
        Debug.Log($"[血條狀態] 目標填充: {targetFillAmount:F2}");

        if (healthBarFill != null)
        {
            Debug.Log($"[血條狀態] 實際填充: {healthBarFill.fillAmount:F2}");
            Debug.Log($"[血條狀態] 血條顏色: {healthBarFill.color}");
        }
    }
}