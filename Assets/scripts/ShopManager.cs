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

    [Header("商品與面板")]
    public List<ShopItem> items;             // 商品清單
    public GameObject shopPanel;             // 商品主面板 (列表)
    public Button[] itemButtons;             // 商品按鈕陣列
    public Button shopCloseButton;           // 商店面板關閉按鈕
    public Button shopButton;                // 外部開啟商店按鈕

    [Header("商品詳情面板")]
    public GameObject detailPanel;           // 詳細面板 (顯示商品資訊)
    public Image selectedItemIcon;           // 顯示商品圖片
    public TextMeshProUGUI selectedItemDescription; // 顯示商品說明
    public Button buyButton;                 // 購買按鈕
    public Button detailCloseButton;         // 關閉詳情面板按鈕

    [Header("玩家金錢")]
    public Text playerMoneyText;             // 金錢顯示

    private int selectedIndex = -1;
    private HashSet<int> purchasedItems = new HashSet<int>();

    void Start()
    {
        // 商店面板、詳情面板初始都不顯示
        shopPanel.SetActive(false);
        detailPanel.SetActive(false);

        // 商品按鈕設定圖片與事件
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

        // 商店面板關閉按鈕點擊事件
        if (shopCloseButton != null)
            shopCloseButton.onClick.AddListener(() => shopPanel.SetActive(false));

        // 開啟商店按鈕點擊事件
        if (shopButton != null)
            shopButton.onClick.AddListener(ToggleShopPanel);

        // 詳情面板關閉按鈕
        if (detailCloseButton != null)
            detailCloseButton.onClick.AddListener(() => detailPanel.SetActive(false));

        // 購買按鈕點擊事件
        if (buyButton != null)
            buyButton.onClick.AddListener(BuySelectedItem);

        LoadPurchasedItems();
        UpdatePlayerMoneyUI();
    }

    void Update()
    {
        // 按下S鍵切換商店面板顯示狀態
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleShopPanel();
        }
    }

    // 切換商店面板顯示/隱藏
    public void ToggleShopPanel()
    {
        bool isActive = shopPanel.activeSelf;
        shopPanel.SetActive(!isActive);

        // 關閉詳情面板，避免UI重疊
        if (detailPanel.activeSelf)
            detailPanel.SetActive(false);
    }

    // 按下商品按鈕呼叫，打開詳情面板，顯示該商品資訊
    public void OnItemButtonClicked(int index)
    {
        if (index < 0 || index >= items.Count)
            return;

        selectedIndex = index;
        ShopItem item = items[index];

        selectedItemIcon.sprite = item.icon;
        selectedItemDescription.text = $"{item.itemName}\n{item.description}\n價格: {item.price}元";

        detailPanel.SetActive(true);
    }

    // 購買當前選中的商品
    public void BuySelectedItem()
    {
        if (selectedIndex == -1)
            return;

        ShopItem item = items[selectedIndex];

        if (purchasedItems.Contains(selectedIndex))
        {
            Debug.Log("已購買過此商品");
            return;
        }

        if (CoinManager.Instance == null)
        {
            Debug.LogError("缺少CoinManager!");
            return;
        }

        if (CoinManager.Instance.MoneyCount >= item.price)
        {
            CoinManager.Instance.AddMoney(-item.price);
            purchasedItems.Add(selectedIndex);
            SavePurchasedItems();
            UpdatePlayerMoneyUI();
            Debug.Log($"購買成功: {item.itemName}");
        }
        else
        {
            Debug.Log("金錢不足");
        }
    }

    // 更新金錢UI顯示
    public void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null && CoinManager.Instance != null)
        {
            playerMoneyText.text = $"金錢: {CoinManager.Instance.MoneyCount}";
        }
    }

    // 儲存已購買商品資料
    public void SavePurchasedItems()
    {
        string data = string.Join(",", purchasedItems);
        PlayerPrefs.SetString("PurchasedItems", data);
        PlayerPrefs.Save();
    }

    // 載入已購買商品資料
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

    // 強制關閉商店及詳情面板，供GameManager調用
    public void ForceCloseShop()
    {
        shopPanel.SetActive(false);
        detailPanel.SetActive(false);
    }
}
