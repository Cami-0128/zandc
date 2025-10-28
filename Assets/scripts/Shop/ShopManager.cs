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
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (player == null || player.isDead) return;
            ToggleShopPanel();
        }
    }

    void InjectEffectsBasedOnName()
    {
        foreach (var item in items)
        {
            Debug.Log($"���ժ`�J�ĪG�G�ӫ~ = {item.itemName}");
            switch (item.itemName)
            {
                case "Health":
                case "�^�_�Ĥ�":
                    item.effect = new HealEffect(20);
                    Debug.Log("�`�JHealEffect");
                    break;
                case "Mana":
                case "�]�O�^�_":
                    item.effect = new ManaHealEffect(20);
                    Debug.Log("�`�JManaHealEffect");
                    break;
                case "CaptureSkill":
                case "�����ޯ�":
                    item.effect = new SkillUnlockEffect(1);
                    Debug.Log("�`�JSkillUnlockEffect");
                    break;
                default:
                    item.effect = null;
                    Debug.Log("���`�J�ĪG");
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
        Debug.Log($"[ShopManager] �R�ӫ~: {item.itemName} ����: {item.price} ����: {CoinManager.Instance.MoneyCount}");

        if (CoinManager.Instance == null)
        {
            Debug.LogError("�ʤ�CoinManager");
            return;
        }
        int moneyCount = CoinManager.Instance.MoneyCount;
        if (moneyCount >= item.price)
        {
            CoinManager.Instance.AddMoney(-item.price);
            UpdatePlayerMoneyUI();
            Debug.Log("[ShopManager] ��������B: " + CoinManager.Instance.MoneyCount);
            if (player != null && item.effect != null)
            {
                Debug.Log("[ShopManager] ����ӫ~�ĪG ApplyEffect");
                item.effect.ApplyEffect(player);
            }
            else
            {
                Debug.LogWarning("player��effect��null");
            }
        }
        else
        {
            Debug.Log("���a���������A�L�k�ʶR");
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