using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public Text moneyText; // ��J UI �� Text �ե�A��ܿ����ƶq

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
        // ���a���`�ɪ��B�z�]�z�]�i�H�b�o�̭��m�����Ψ�L�ĪG�^
    }
}
