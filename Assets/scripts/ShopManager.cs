using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public string description;
        public int price;
        public Sprite icon;
    }

    [Header("�ӫ~�P���O")]
    public List<ShopItem> items;             // �ӫ~�M��
    public GameObject shopPanel;             // �ӫ~�D���O (�C��)
    public Button[] itemButtons;             // �ӫ~���s�}�C
    public Button shopCloseButton;           // �ө����O�������s
    public Button shopButton;                // �~���}�Ұө����s

    [Header("�ӫ~�Ա����O")]
    public GameObject detailPanel;           // �Բӭ��O (��ܰӫ~��T)
    public Image selectedItemIcon;           // ��ܰӫ~�Ϥ�
    public TextMeshProUGUI selectedItemDescription; // ��ܰӫ~����
    public Button buyButton;                 // �ʶR���s
    public Button detailCloseButton;         // �����Ա����O���s

    [Header("���a����")]
    public Text playerMoneyText;             // �������

    private int selectedIndex = -1;
    private HashSet<int> purchasedItems = new HashSet<int>();

    void Start()
    {
        // �ө����O�B�Ա����O��l�������
        shopPanel.SetActive(false);
        detailPanel.SetActive(false);

        // �ӫ~���s�]�w�Ϥ��P�ƥ�
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            if (index < items.Count)
            {
                itemButtons[i].image.sprite = items[index].icon;
                itemButtons[i].onClick.AddListener(() => OnItemButtonClicked(index));
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }

        // �ө����O�������s�I���ƥ�
        if (shopCloseButton != null)
            shopCloseButton.onClick.AddListener(() => shopPanel.SetActive(false));

        // �}�Ұө����s�I���ƥ�
        if (shopButton != null)
            shopButton.onClick.AddListener(ToggleShopPanel);

        // �Ա����O�������s
        if (detailCloseButton != null)
            detailCloseButton.onClick.AddListener(() => detailPanel.SetActive(false));

        // �ʶR���s�I���ƥ�
        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedItem);

        LoadPurchasedItems();
        UpdatePlayerMoneyUI();
    }

    void Update()
    {
        // ���US������ө����O��ܪ��A
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleShopPanel();
        }
    }

    // �����ө����O���/����
    public void ToggleShopPanel()
    {
        bool isActive = shopPanel.activeSelf;
        shopPanel.SetActive(!isActive);

        // �����Ա����O�A�קKUI���|
        if (detailPanel.activeSelf)
            detailPanel.SetActive(false);
    }

    // ���U�ӫ~���s�I�s�A���}�Ա����O�A��ܸӰӫ~��T
    public void OnItemButtonClicked(int index)
    {
        if (index < 0 || index >= items.Count)
            return;

        selectedIndex = index;
        ShopItem item = items[index];

        selectedItemIcon.sprite = item.icon;
        selectedItemDescription.text = $"{item.itemName}\n{item.description}\n����: {item.price}��";

        detailPanel.SetActive(true);
    }

    // �ʶR��e�襤���ӫ~
    public void BuySelectedItem()
    {
        if (selectedIndex == -1)
            return;

        ShopItem item = items[selectedIndex];

        if (purchasedItems.Contains(selectedIndex))
        {
            Debug.Log("�w�ʶR�L���ӫ~");
            return;
        }

        if (CoinManager.Instance == null)
        {
            Debug.LogError("�ʤ�CoinManager!");
            return;
        }

        if (CoinManager.Instance.MoneyCount >= item.price)
        {
            CoinManager.Instance.AddMoney(-item.price);
            purchasedItems.Add(selectedIndex);
            SavePurchasedItems();
            UpdatePlayerMoneyUI();
            Debug.Log($"�ʶR���\: {item.itemName}");
        }
        else
        {
            Debug.Log("��������");
        }
    }

    // ��s����UI���
    public void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null && CoinManager.Instance != null)
        {
            playerMoneyText.text = $"����: {CoinManager.Instance.MoneyCount}";
        }
    }

    // �x�s�w�ʶR�ӫ~���
    public void SavePurchasedItems()
    {
        string data = string.Join(",", purchasedItems);
        PlayerPrefs.SetString("PurchasedItems", data);
        PlayerPrefs.Save();
    }

    // ���J�w�ʶR�ӫ~���
    public void LoadPurchasedItems()
    {
        string data = PlayerPrefs.GetString("PurchasedItems", "");
        purchasedItems.Clear();
        if (!string.IsNullOrEmpty(data))
        {
            string[] parts = data.Split(',');
            foreach (var p in parts)
            {
                if (int.TryParse(p, out int idx))
                    purchasedItems.Add(idx);
            }
        }
    }

    // �j�������ө��θԱ����O�A��GameManager�ե�
    public void ForceCloseShop()
    {
        shopPanel.SetActive(false);
        detailPanel.SetActive(false);
    }
}
