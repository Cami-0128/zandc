using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    public Text moneyText;   // �b Inspector ��J UI �� Text �ե�

    private int moneyCount = 0;
    public int MoneyCount { get { return moneyCount; } }

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

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "Money : " + moneyCount.ToString();
    }
}
