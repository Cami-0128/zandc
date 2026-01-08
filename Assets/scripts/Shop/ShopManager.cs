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
    public GameObject detailPanel;  // SelectedPanel
    public Image selectedItemIcon;
    public TextMeshProUGUI selectedItemDescription;
    public Button buyButton;
    public Button detailCloseButton;
    public Text playerMoneyText;

    private int selectedIndex = -1;
    private PlayerController2D player;

    void Start()
    {
        player = FindObjectOfType<PlayerController2D>();
        if (shopPanel != null) shopPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);

        InjectEffectsBasedOnName();
        SetupUIButtons();

        if (shopCloseButton != null)
        {
            shopCloseButton.onClick.RemoveAllListeners();
            shopCloseButton.onClick.AddListener(() => CloseShopPanel());
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
        bool shopKeyPressed = false;

        // 優先使用自定義按鍵
        if (KeyBindingManager.Instance != null)
        {
            shopKeyPressed = KeyBindingManager.Instance.GetKeyDown(KeyBindingManager.ActionType.OpenShop);
        }
        else
        {
            // 回退到傳統按鍵
            shopKeyPressed = Input.GetKeyDown(KeyCode.S);
        }

        if (shopKeyPressed)
        {
            if (player == null || player.isDead) return;
            ToggleShopPanel();
        }
    }

    void InjectEffectsBasedOnName()
    {
        foreach (var item in items)
        {
            Debug.Log($"嘗試注入效果：商品 = {item.itemName}");
            switch (item.itemName)
            {
                case "Health":
                case "回復藥水":
                    item.effect = new HealEffect(20);
                    Debug.Log("注入HealEffect");
                    break;
                case "Mana":
                case "魔力回復":
                    item.effect = new ManaHealEffect(20);
                    Debug.Log("注入ManaHealEffect");
                    break;
                case "CaptureSkill":
                case "捕捉技能":
                    item.effect = new SkillUnlockEffect(1);
                    Debug.Log("注入SkillUnlockEffect");
                    break;

                // ========== 新增：彈簧商品（簡單版） ==========
                case "Bouncer":
                case "彈簧":
                case "彈跳器":
                    // 檢查是否有 BouncerSpawnManager
                    if (BouncerSpawnManager.Instance != null)
                    {
                        item.effect = BouncerSpawnManager.Instance.CreateBouncerEffect();
                        Debug.Log("注入SimpleBouncerEffect（從 BouncerSpawnManager）");
                    }
                    else
                    {
                        Debug.LogError("缺少 BouncerSpawnManager！請在場景中創建並設定。");
                        item.effect = null;
                    }
                    break;
                // =============================================

                default:
                    item.effect = null;
                    Debug.Log("未注入效果");
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

    public void OnItemButtonClicked(int index)
    {
        if (player == null || player.isDead) return;
        if (index < 0 || index >= items.Count) return;
        selectedIndex = index;
        ShopItem item = items[index];
        if (selectedItemIcon != null) selectedItemIcon.sprite = item.icon;
        if (selectedItemDescription != null)
            selectedItemDescription.text = $"{item.itemName}\n{item.description}\nPrice : {item.price} coin";
        if (detailPanel != null)
            detailPanel.SetActive(true);
    }

    void ToggleShopPanel()
    {
        if (player == null || player.isDead)
        {
            CloseShopPanel();
            return;
        }
        if (shopPanel == null) return;
        bool isActive = shopPanel.activeSelf;
        if (isActive)
        {
            CloseShopPanel();
        }
        else
        {
            shopPanel.SetActive(true);
        }
    }

    void CloseShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    public void BuySelectedItem()
    {
        if (selectedIndex == -1) return;
        var item = items[selectedIndex];
        Debug.Log($"[ShopManager] 買商品: {item.itemName} 價格: {item.price} 金錢: {CoinManager.Instance.MoneyCount}");

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
            Debug.Log("[ShopManager] 扣錢後金額: " + CoinManager.Instance.MoneyCount);
            if (player != null && item.effect != null)
            {
                Debug.Log("[ShopManager] 執行商品效果 ApplyEffect");
                item.effect.ApplyEffect(player);
            }
            else
            {
                Debug.LogWarning("player或effect為null");
            }
        }
        else
        {
            Debug.Log("玩家金錢不足，無法購買");
        }
    }

    public void UpdatePlayerMoneyUI()
    {
        if (playerMoneyText != null && CoinManager.Instance != null)
            playerMoneyText.text = $"Money : {CoinManager.Instance.MoneyCount}";
    }

    public void ForceCloseShop()
    {
        CloseShopPanel();
    }
}