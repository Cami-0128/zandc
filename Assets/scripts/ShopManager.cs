using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<ShopItem> items;

    public GameObject shopPanel;
    public Button[] itemButtons;
    public Button shopCloseButton;
    public Button shopButton;

    public GameObject detailPanel;
    public Image selectedItemIcon;
    public TextMeshProUGUI selectedItemDescription;
    public Button buyButton;
    public Button detailCloseButton;

    public Text playerMoneyText;

    private int selectedIndex = -1;

    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);

        InjectEffectsBasedOnName();
        SetupUIButtons();

        if (shopCloseButton != null)
        {
            shopCloseButton.onClick.RemoveAllListeners();
            shopCloseButton.onClick.AddListener(() => shopPanel.SetActive(false));
        }
        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(ToggleShopPanel);
        }
        if (detailCloseButton != null)
        {
            detailCloseButton.onClick.RemoveAllListeners();
            detailCloseButton.onClick.AddListener(() => detailPanel.SetActive(false));
        }
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuySelectedItem);
        }

        UpdatePlayerMoneyUI();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            ToggleShopPanel();
    }

    void InjectEffectsBasedOnName()
    {
        foreach (var item in items)
        {
            switch (item.itemName)
            {
                case "Health":
                case "回復藥水":
                    item.effect = new HealEffect(20); // 回復數量寫死
                    break;
                default:
                    item.effect = null;
                    break;
            }
        }
    }

    void SetupUIButtons()
    {
        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            if (index < items.Count)
            {
                itemButtons[i].image.sprite = items[index].icon;
                itemButtons[i].onClick.RemoveAllListeners();
                itemButtons[i].onClick.AddListener(() => OnItemButtonClicked(index));
                itemButtons[i].gameObject.SetActive(true);
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void ToggleShopPanel()
    {
        bool isActive = shopPanel.activeSelf;
        shopPanel.SetActive(!isActive);
        if (detailPanel != null && detailPanel.activeSelf)
            detailPanel.SetActive(false);
    }

    public void OnItemButtonClicked(int index)
    {
        if (index < 0 || index >= items.Count)
            return;

        selectedIndex = index;
        ShopItem item = items[index];
        if (selectedItemIcon != null) selectedItemIcon.sprite = item.icon;
        if (selectedItemDescription != null)
            selectedItemDescription.text = $"{item.itemName}\n{item.description}\n價格: {item.price}元";
        if (detailPanel != null)
            detailPanel.SetActive(true);
    }

    public void BuySelectedItem()
    {
        if (selectedIndex == -1) return;
        var item = items[selectedIndex];

        if (CoinManager.Instance == null)
        {
            Debug.LogError("缺少CoinManager");
            return;
        }

        int moneyCount = CoinManager.Instance.MoneyCount;
        if (moneyCount >= item.price)
        {
            CoinManager.Instance.AddMoney(-item.price);
            UpdatePlayerMoneyUI();

            PlayerController2D player = FindObjectOfType<PlayerController2D>();
            if (player != null && item.effect != null)
            {
                item.effect.ApplyEffect(player);
            }
        }
    }

    public void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null && CoinManager.Instance != null)
            playerMoneyText.text = $"Money : {CoinManager.Instance.MoneyCount}";
    }
}
