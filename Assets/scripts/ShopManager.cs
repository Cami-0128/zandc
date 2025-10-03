using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public List<ShopItem> items;

    public GameObject shopPanel;
    public Animator shopAnimator;
    public Button[] itemButtons;
    public Image selectedItemIcon;
    public Text selectedItemDescription;
    public Text playerMoneyText;

    public Button openShopButton;   // 代表商店的開啟按鈕
    public Button closeButton;      // 商店關閉按鈕

    private int selectedIndex = -1;
    private HashSet<int> purchasedItems = new HashSet<int>();

    private bool isShopOpen = false;

    void Start()
    {
        shopPanel.SetActive(false);

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

        if (openShopButton != null)
            openShopButton.onClick.AddListener(() => { if (!isShopOpen) OpenShop(); });

        if (closeButton != null)
            closeButton.onClick.AddListener(() => { if (isShopOpen) CloseShop(); });

        LoadPurchasedItems();
        UpdatePlayerMoneyUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isShopOpen)
                CloseShop();
            else
                OpenShop();
        }
    }

    public void OpenShop()
    {
        isShopOpen = true;
        shopPanel.SetActive(true);
        shopAnimator.SetTrigger("Open");
    }

    public void CloseShop()
    {
        if (!shopPanel.activeSelf) return;

        isShopOpen = false;
        shopAnimator.SetTrigger("Close");
        Invoke(nameof(DeactivateShopPanel), 0.4f); // 動畫長度依實際調整
    }

    private void DeactivateShopPanel()
    {
        shopPanel.SetActive(false);
    }

    public void OnItemButtonClicked(int index)
    {
        if (index < 0 || index >= items.Count) return;

        selectedIndex = index;
        var item = items[index];
        selectedItemIcon.sprite = item.icon;
        selectedItemDescription.text = $"{item.itemName}\n{item.description}\n價格: {item.price}元";
    }

    public void BuySelectedItem()
    {
        if (selectedIndex == -1) return;

        var item = items[selectedIndex];
        if (purchasedItems.Contains(selectedIndex))
        {
            Debug.Log("該商品已購買");
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

    void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null && CoinManager.Instance != null)
        {
            playerMoneyText.text = $"金錢: {CoinManager.Instance.MoneyCount}";
        }
    }

    void SavePurchasedItems()
    {
        string data = string.Join(",", purchasedItems);
        PlayerPrefs.SetString("PurchasedItems", data);
        PlayerPrefs.Save();
    }

    void LoadPurchasedItems()
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
}
