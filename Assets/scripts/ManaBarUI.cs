using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("魔力條UI組件")]
    public Image manaBarFill;
    public Image manaBarBackground;
    public TextMeshProUGUI manaText;

    [Header("視覺效果 - 魔力條顏色漸變")]
    [Tooltip("100-76% 魔力時的顏色")]
    public Color fullManaColor = new Color(0.2f, 0.5f, 1f);      // 藍色

    [Tooltip("75-51% 魔力時的顏色")]
    public Color highManaColor = new Color(0.5f, 0.3f, 0.8f);    // 紫色

    [Tooltip("50-26% 魔力時的顏色")]
    public Color mediumManaColor = new Color(0.4f, 0.7f, 1f);    // 淺藍

    [Tooltip("25-0% 魔力時的顏色")]
    public Color lowManaColor = new Color(0.1f, 0.2f, 0.5f);     // 深藍

    [Header("動畫設定")]
    public bool enableSmoothTransition = true;
    public float transitionSpeed = 5f;
    public bool enableFlashOnUse = true;
    public Color useFlashColor = Color.cyan;
    public float flashDuration = 0.15f;

    [Header("顏色漸變設定")]
    public bool enableColorLerp = true;
    public float colorTransitionSpeed = 3f;

    private PlayerAttack playerAttack;
    private float targetFillAmount;
    private float currentFillAmount;
    private int lastKnownMana = -1;
    private Color currentColor;
    private Color targetColor;
    private bool isFlashing = false;
    private bool isInitialized = false;
    private Coroutine flashCoroutine;

    // === 關鍵：在 Editor 中就顯示滿的狀態 ===
    void OnValidate()
    {
        // 這個方法會在 Inspector 中修改任何值時執行
        if (manaBarFill != null)
        {
            manaBarFill.type = Image.Type.Filled;
            manaBarFill.fillMethod = Image.FillMethod.Horizontal;
            manaBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            manaBarFill.fillAmount = 1f;  // 預設顯示滿的
            manaBarFill.color = fullManaColor;
        }

        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Awake()
    {
        // === 在 Start 之前就設定好初始值，避免閃爍 ===
        currentFillAmount = 1f;
        targetFillAmount = 1f;
        currentColor = fullManaColor;
        targetColor = fullManaColor;

        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = 1f;
            manaBarFill.color = fullManaColor;
        }

        if (manaText != null)
        {
            manaText.text = "100/100";
        }
    }

    void Start()
    {
        StartCoroutine(InitializeManaBar());
    }

    // === 初始化協程（參考你的 HP 條寫法）===
    IEnumerator InitializeManaBar()
    {
        // 等待一幀，確保玩家已經初始化
        yield return new WaitForEndOfFrame();

        // 尋找玩家攻擊組件
        playerAttack = FindObjectOfType<PlayerAttack>();

        // 等待玩家載入
        int attempts = 0;
        while (playerAttack == null && attempts < 100)
        {
            yield return new WaitForEndOfFrame();
            playerAttack = FindObjectOfType<PlayerAttack>();
            attempts++;
        }

        if (playerAttack != null)
        {
            // 根據玩家的實際魔力初始化魔力條
            int currentMana = playerAttack.GetCurrentMana();
            int maxMana = playerAttack.maxMana;

            Debug.Log($"[魔力條初始化] 載入玩家魔力: {currentMana}/{maxMana}");

            // 計算魔力百分比
            float manaPercentage = (float)currentMana / maxMana;

            // 設置魔力條
            currentFillAmount = manaPercentage;
            targetFillAmount = manaPercentage;

            if (manaBarFill != null)
            {
                manaBarFill.fillAmount = manaPercentage;
                UpdateManaBarColor(manaPercentage);
                Debug.Log($"魔力條初始化完成 - 魔力: {currentMana}/{maxMana} ({manaPercentage:P0})");
            }

            // 更新魔力文字
            UpdateManaText(currentMana, maxMana);

            // 記錄初始魔力
            lastKnownMana = currentMana;
            isInitialized = true;
        }
        else
        {
            Debug.LogError("找不到 PlayerAttack 組件！魔力條無法正確初始化。");

            // 備用方案：設為滿魔力
            if (manaBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                manaBarFill.fillAmount = 1f;
                manaBarFill.color = fullManaColor;
                Debug.Log("使用備用初始化方案，設置魔力條為滿魔力");
            }
        }
    }

    void Update()
    {
        // 自動同步魔力
        if (isInitialized)
        {
            AutoSyncWithPlayer();
        }

        // 平滑過渡動畫
        if (enableSmoothTransition && manaBarFill != null)
        {
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.01f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, transitionSpeed * Time.deltaTime);
                manaBarFill.fillAmount = currentFillAmount;

                // 確保魔力為0時完全隱藏
                if (targetFillAmount <= 0f && currentFillAmount < 0.01f)
                {
                    currentFillAmount = 0f;
                    manaBarFill.fillAmount = 0f;
                }
            }
        }

        // 平滑更新顏色
        if (enableColorLerp && manaBarFill != null && !isFlashing)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            manaBarFill.color = currentColor;
        }
    }

    // === 自動同步玩家魔力 ===
    void AutoSyncWithPlayer()
    {
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();

            // 如果魔力有變化，自動更新魔力條
            if (currentMana != lastKnownMana)
            {
                int maxMana = playerAttack.maxMana;
                Debug.Log($"[自動同步] 檢測到魔力變化: {lastKnownMana} → {currentMana}");
                UpdateManaBar(currentMana, maxMana);
                lastKnownMana = currentMana;
            }
        }
    }

    // === 主要功能：更新魔力條 ===
    public void UpdateManaBar(int currentMana, int maxMana)
    {
        Debug.Log($"ManaBarUI.UpdateManaBar 被呼叫 - 魔力: {currentMana}/{maxMana}");

        if (manaBarFill == null)
        {
            Debug.LogError("魔力條填充圖片未設置！");
            return;
        }

        // 計算魔力百分比
        float manaPercentage = (float)currentMana / maxMana;
        Debug.Log($"魔力百分比: {manaPercentage:F2} ({manaPercentage:P0})");

        // 檢測是否使用魔力（用於閃爍效果）
        bool usedMana = (lastKnownMana > 0 && currentMana < lastKnownMana);

        // 設置目標填充量
        targetFillAmount = manaPercentage;

        // 如果不使用平滑過渡，直接設置
        if (!enableSmoothTransition)
        {
            currentFillAmount = targetFillAmount;
            manaBarFill.fillAmount = currentFillAmount;
        }

        // 強制設置魔力為0時的填充量
        if (currentMana <= 0)
        {
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            manaBarFill.fillAmount = 0f;
        }

        // 更新魔力條顏色
        UpdateManaBarColor(manaPercentage);

        // 更新魔力文字
        UpdateManaText(currentMana, maxMana);

        // 只有使用魔力時才播放閃爍效果
        if (enableFlashOnUse && usedMana)
        {
            PlayManaFlash();
            Debug.Log($"檢測到使用魔力，播放閃爍效果 ({lastKnownMana} → {currentMana})");
        }

        // 更新記錄的魔力
        lastKnownMana = currentMana;

        // 標記為已初始化
        if (!isInitialized)
        {
            isInitialized = true;
        }
    }

    // === 更新魔力條顏色 ===
    void UpdateManaBarColor(float manaPercentage)
    {
        if (manaBarFill == null) return;

        Color newTargetColor;
        string colorName;

        if (manaPercentage > 0.75f)
        {
            newTargetColor = fullManaColor;
            colorName = "藍色";
        }
        else if (manaPercentage > 0.5f)
        {
            newTargetColor = highManaColor;
            colorName = "紫色";
        }
        else if (manaPercentage > 0.25f)
        {
            newTargetColor = mediumManaColor;
            colorName = "淺藍";
        }
        else
        {
            newTargetColor = lowManaColor;
            colorName = "深藍";
        }

        targetColor = newTargetColor;

        if (!enableColorLerp)
        {
            currentColor = targetColor;
            manaBarFill.color = targetColor;
        }

        Debug.Log($"魔力條顏色變更為：{colorName} (魔力百分比: {manaPercentage:P0})");
    }

    // === 更新魔力文字 ===
    void UpdateManaText(int currentMana, int maxMana)
    {
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }

    // === 播放使用魔力閃爍效果 ===
    void PlayManaFlash()
    {
        // 如果已經在閃爍，先停止
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (manaBarFill != null)
        {
            flashCoroutine = StartCoroutine(ManaFlashCoroutine());
            Debug.Log("開始魔力使用閃爍效果");
        }
    }

    // === 閃爍協程 ===
    IEnumerator ManaFlashCoroutine()
    {
        if (manaBarFill == null)
        {
            Debug.LogWarning("閃爍效果：找不到魔力條填充圖片");
            yield break;
        }

        isFlashing = true;

        // 記住原來的顏色
        Color originalColor = currentColor;

        // 變成閃爍顏色
        manaBarFill.color = useFlashColor;

        // 等待閃爍時間
        yield return new WaitForSeconds(flashDuration);

        // 恢復原來的顏色
        manaBarFill.color = originalColor;

        isFlashing = false;
        flashCoroutine = null;
        Debug.Log("閃爍效果結束，恢復原始顏色");
    }

    // === 魔力條重置 ===
    public void ResetManaBar()
    {
        PlayerAttack player = FindObjectOfType<PlayerAttack>();

        if (player != null)
        {
            int currentMana = player.GetCurrentMana();
            int maxMana = player.maxMana;

            Debug.Log($"[魔力條重置] 重置為玩家當前魔力: {currentMana}/{maxMana}");
            UpdateManaBar(currentMana, maxMana);
        }
        else
        {
            // 備用方案：設為滿魔力
            if (manaBarFill != null)
            {
                currentFillAmount = 1f;
                targetFillAmount = 1f;
                manaBarFill.fillAmount = 1f;
                UpdateManaBarColor(1f);
            }

            if (manaText != null)
            {
                manaText.text = "100/100";
            }

            Debug.Log("魔力條重置為預設滿魔力狀態");
        }

        // 重置狀態
        lastKnownMana = -1;
        isInitialized = false;

        // 重新初始化
        StartCoroutine(InitializeManaBar());
    }

    // === 強制同步魔力 ===
    public void ForceSync()
    {
        if (playerAttack != null)
        {
            int currentMana = playerAttack.GetCurrentMana();
            int maxMana = playerAttack.maxMana;

            Debug.Log($"[強制同步] 同步魔力: {currentMana}/{maxMana}");
            UpdateManaBar(currentMana, maxMana);
        }
    }
}