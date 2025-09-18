using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public Text moneyText; // 拖入 UI 的 Text 組件，顯示錢幣數量

    private int moneyCount = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddMoney(int amount)
    {
        moneyCount += amount;
        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + moneyCount.ToString();
        }
    }

    public void PlayerDied()
    {
        // 玩家死亡時的處理（您也可以在這裡重置錢幣或其他效果）
    }
}
