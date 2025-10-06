using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 金幣管理器 - 增強版
/// 管理玩家的金幣數量
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("UI 設定")]
    public Text moneyText;   // 在 Inspector 拖入 UI 的 Text 組件

    [Header("金幣數據")]
    private int moneyCount = 0;

    public int MoneyCount
    {
        get { return moneyCount; }
    }

    private void Awake()
    {
        // Singleton 模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 可選：切換場景時不銷毀
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    /// <summary>
    /// 增加金幣
    /// </summary>
    public void AddMoney(int amount)
    {
        moneyCount += amount;
        UpdateMoneyUI();
        Debug.Log($"[CoinManager] 獲得 {amount} 金幣，總計: {moneyCount}");
    }

    /// <summary>
    /// 扣除金幣（購買道具用）
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (moneyCount >= amount)
        {
            moneyCount -= amount;
            UpdateMoneyUI();
            Debug.Log($"[CoinManager] 花費 {amount} 金幣，剩餘: {moneyCount}");
            return true;
        }
        else
        {
            Debug.Log("[CoinManager] 金幣不足！");
            return false;
        }
    }

    /// <summary>
    /// 設定金幣數量
    /// </summary>
    public void SetMoney(int amount)
    {
        moneyCount = amount;
        UpdateMoneyUI();
    }

    /// <summary>
    /// 更新 UI 顯示
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + moneyCount.ToString();
        }
    }

    /// <summary>
    /// 檢查是否有足夠的金幣
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return moneyCount >= amount;
    }
}