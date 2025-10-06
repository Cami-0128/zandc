using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �����޲z�� - �W�j��
/// �޲z���a�������ƶq
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    [Header("UI �]�w")]
    public Text moneyText;   // �b Inspector ��J UI �� Text �ե�

    [Header("�����ƾ�")]
    private int moneyCount = 0;

    public int MoneyCount
    {
        get { return moneyCount; }
    }

    private void Awake()
    {
        // Singleton �Ҧ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �i��G���������ɤ��P��
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
    /// �W�[����
    /// </summary>
    public void AddMoney(int amount)
    {
        moneyCount += amount;
        UpdateMoneyUI();
        Debug.Log($"[CoinManager] ��o {amount} �����A�`�p: {moneyCount}");
    }

    /// <summary>
    /// ���������]�ʶR�D��Ρ^
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (moneyCount >= amount)
        {
            moneyCount -= amount;
            UpdateMoneyUI();
            Debug.Log($"[CoinManager] ��O {amount} �����A�Ѿl: {moneyCount}");
            return true;
        }
        else
        {
            Debug.Log("[CoinManager] ���������I");
            return false;
        }
    }

    /// <summary>
    /// �]�w�����ƶq
    /// </summary>
    public void SetMoney(int amount)
    {
        moneyCount = amount;
        UpdateMoneyUI();
    }

    /// <summary>
    /// ��s UI ���
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: " + moneyCount.ToString();
        }
    }

    /// <summary>
    /// �ˬd�O�_������������
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return moneyCount >= amount;
    }
}