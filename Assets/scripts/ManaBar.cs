using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaBarUI : MonoBehaviour
{
    [Header("魔力條UI組件")]
    [SerializeField] private Image manaBarFill;
    [SerializeField] private Image manaBarBackground;
    [SerializeField] private TextMeshProUGUI manaText;

    [Header("視覺效果 - 魔力條顏色漸變")]
    [Tooltip("滿魔力顏色 (100%-76%)")]
    [SerializeField] private Color fullManaColor = new Color(0.2f, 0.5f, 1f); // 藍色

    [Tooltip("高魔力顏色 (75%-51%)")]
    [SerializeField] private Color highManaColor = new Color(0.5f, 0.3f, 0.8f); // 紫色

    [Tooltip("中魔力顏色 (50%-26%)")]
    [SerializeField] private Color mediumManaColor = new Color(0.4f, 0.7f, 1f); // 淺藍

    [Tooltip("低魔力顏色 (25%-0%)")]
    [SerializeField] private Color lowManaColor = new Color(0.1f, 0.2f, 0.5f); // 深藍

    [Header("平滑過渡設定")]
    [Tooltip("啟用顏色平滑過渡")]
    [SerializeField] private bool enableColorLerp = true;

    [Tooltip("顏色過渡速度")]
    [SerializeField] private float colorTransitionSpeed = 5f;

    [Header("視覺回饋")]
    [Tooltip("使用魔力時閃爍效果")]
    [SerializeField] private bool enableFlashOnUse = true;

    [Tooltip("閃爍持續時間")]
    [SerializeField] private float flashDuration = 0.2f;

    private PlayerAttack playerAttack;
    private Color currentColor;
    private Color targetColor;
    private int lastMana;
    private bool isFlashing = false;

    void Awake()
    {
        // 尋找 PlayerAttack 組件
        playerAttack = FindObjectOfType<PlayerAttack>();

        if (playerAttack == null)
        {
            Debug.LogError("找不到 PlayerAttack 組件！請確保場景中有玩家物件並附加 PlayerAttack 腳本。");
            enabled = false;
            return;
        }

        // 驗證 UI 組件
        if (manaBarFill == null || manaBarBackground == null || manaText == null)
        {
            Debug.LogError("ManaBarUI 組件未完整設定！請在 Inspector 中指定所有 UI 元件。");
            enabled = false;
            return;
        }

        // 初始化
        lastMana = playerAttack.GetCurrentMana();
        UpdateManaBarImmediate();
    }

    void Update()
    {
        int currentMana = playerAttack.GetCurrentMana();

        // 檢測魔力消耗
        if (enableFlashOnUse && currentMana < lastMana && !isFlashing)
        {
            StartCoroutine(FlashEffect());
        }

        lastMana = currentMana;
        UpdateManaBar();
    }

    /// <summary>
    /// 更新魔力條（平滑版本）
    /// </summary>
    private void UpdateManaBar()
    {
        int currentMana = playerAttack.GetCurrentMana();
        int maxMana = playerAttack.maxMana;
        float currentManaPercentage = (float)currentMana / maxMana;

        // 更新填充量
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = currentManaPercentage;
        }

        // 更新文字
        if (manaText != null)
        {
            manaText.text = currentMana + "/" + maxMana;
        }

        // 更新顏色
        targetColor = GetColorForManaPercentage(currentManaPercentage);

        if (enableColorLerp)
        {
            currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorTransitionSpeed);
        }
        else
        {
            currentColor = targetColor;
        }

        if (manaBarFill != null)
        {
            manaBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// 立即更新魔力條（無動畫，用於初始化）
    /// </summary>
    private void UpdateManaBarImmediate()
    {
        int currentMana = playerAttack.GetCurrentMana();
        int maxMana = playerAttack.maxMana;
        float currentManaPercentage = (float)currentMana / maxMana;

        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = currentManaPercentage;
        }

        if (manaText != null)
        {
            manaText.text = currentMana + "/" + maxMana;
        }

        currentColor = GetColorForManaPercentage(currentManaPercentage);
        targetColor = currentColor;

        if (manaBarFill != null)
        {
            manaBarFill.color = currentColor;
        }
    }

    /// <summary>
    /// 根據魔力百分比獲取對應顏色
    /// </summary>
    private Color GetColorForManaPercentage(float percentage)
    {
        if (percentage > 0.75f)
            return fullManaColor;
        else if (percentage > 0.50f)
            return highManaColor;
        else if (percentage > 0.25f)
            return mediumManaColor;
        else
            return lowManaColor;
    }

    /// <summary>
    /// 閃爍效果協程
    /// </summary>
    private System.Collections.IEnumerator FlashEffect()
    {
        isFlashing = true;
        Color originalColor = manaBarFill.color;

        // 變白
        manaBarFill.color = Color.white;
        yield return new UnityEngine.WaitForSeconds(flashDuration / 2);

        // 恢復原色
        manaBarFill.color = originalColor;
        yield return new UnityEngine.WaitForSeconds(flashDuration / 2);

        isFlashing = false;
    }
}