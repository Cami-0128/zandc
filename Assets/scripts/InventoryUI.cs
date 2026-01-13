using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour //廢?
{
    [Header("UI 組件")]
    public GameObject inventoryPanel;
    public Button inventoryButton;
    public Button closeButton;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    [Header("詳細資訊面板")]
    public GameObject detailPanel;
    public Image detailItemIcon;
    public TextMeshProUGUI detailItemName;
    public TextMeshProUGUI detailItemDescription;
    public Button detailCloseButton;

    [Header("分類篩選")]
    public Toggle allItemsToggle;
    public Toggle keysToggle;
    public Toggle consumablesToggle;
    public Toggle materialsToggle;

    private PlayerInventory playerInventory;
    private PlayerController2D player;
    private List<GameObject> currentSlots = new List<GameObject>();
    private InventoryItem.ItemType currentFilter = InventoryItem.ItemType.Key;

    void Start()
    {
        playerInventory = FindObjectOfType<PlayerInventory>();
        player = FindObjectOfType<PlayerController2D>();

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        SetupButtons();
        SetupToggles();
        UpdateInventoryDisplay();
    }

    void Update()
    {
        bool inventoryKeyPressed = false;

        // 優先使用自定義按鍵
        if (KeyBindingManager.Instance != null)
        {
            // 假設你要新增 OpenInventory 到 KeyBindingManager
            inventoryKeyPressed = Input.GetKeyDown(KeyCode.B);
        }
        else
        {
            inventoryKeyPressed = Input.GetKeyDown(KeyCode.B);
        }

        if (inventoryKeyPressed)
        {
            if (player == null || player.isDead) return;
            ToggleInventoryPanel();
        }
    }

    void SetupButtons()
    {
        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(ToggleInventoryPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => CloseInventoryPanel());
        }

        if (detailCloseButton != null)
        {
            detailCloseButton.onClick.RemoveAllListeners();
            detailCloseButton.onClick.AddListener(() => detailPanel.SetActive(false));
        }
    }

    void SetupToggles()
    {
        if (allItemsToggle != null)
        {
            allItemsToggle.onValueChanged.RemoveAllListeners();
            allItemsToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) ShowAllItems();
            });
        }

        if (keysToggle != null)
        {
            keysToggle.onValueChanged.RemoveAllListeners();
            keysToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) FilterByType(InventoryItem.ItemType.Key);
            });
            keysToggle.isOn = true; // 預設顯示鑰匙
        }

        if (consumablesToggle != null)
        {
            consumablesToggle.onValueChanged.RemoveAllListeners();
            consumablesToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) FilterByType(InventoryItem.ItemType.Consumable);
            });
        }

        if (materialsToggle != null)
        {
            materialsToggle.onValueChanged.RemoveAllListeners();
            materialsToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) FilterByType(InventoryItem.ItemType.Material);
            });
        }
    }

    void ToggleInventoryPanel()
    {
        if (player == null || player.isDead)
        {
            CloseInventoryPanel();
            return;
        }

        if (inventoryPanel == null) return;

        bool isActive = inventoryPanel.activeSelf;
        if (isActive)
        {
            CloseInventoryPanel();
        }
        else
        {
            inventoryPanel.SetActive(true);
            UpdateInventoryDisplay();
        }
    }

    void CloseInventoryPanel()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);
    }

    public void UpdateInventoryDisplay()
    {
        if (playerInventory == null || itemContainer == null) return;

        // 清空現有格子
        foreach (GameObject slot in currentSlots)
        {
            Destroy(slot);
        }
        currentSlots.Clear();

        // 獲取要顯示的物品
        List<InventoryItem> itemsToDisplay = GetFilteredItems();

        // 創建物品格子
        foreach (InventoryItem item in itemsToDisplay)
        {
            CreateItemSlot(item);
        }
    }

    List<InventoryItem> GetFilteredItems()
    {
        if (playerInventory == null)
            return new List<InventoryItem>();

        // 如果選擇顯示全部
        if (allItemsToggle != null && allItemsToggle.isOn)
        {
            return playerInventory.GetAllItems();
        }

        // 根據類型篩選
        return playerInventory.GetItemsByType(currentFilter);
    }

    void CreateItemSlot(InventoryItem item)
    {
        if (itemSlotPrefab == null) return;

        GameObject slot = Instantiate(itemSlotPrefab, itemContainer);
        currentSlots.Add(slot);

        // 設定圖標
        Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null && item.itemIcon != null)
        {
            icon.sprite = item.itemIcon;
        }

        // 設定數量
        TextMeshProUGUI quantityText = slot.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
        if (quantityText != null)
        {
            quantityText.text = item.quantity > 1 ? $"x{item.quantity}" : "";
        }

        // 設定名稱
        TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = item.itemName;
        }

        // 設定點擊事件
        Button slotButton = slot.GetComponent<Button>();
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => ShowItemDetail(item));
        }
    }

    void ShowItemDetail(InventoryItem item)
    {
        if (detailPanel == null) return;

        detailPanel.SetActive(true);

        if (detailItemIcon != null && item.itemIcon != null)
        {
            detailItemIcon.sprite = item.itemIcon;
        }

        if (detailItemName != null)
        {
            detailItemName.text = item.itemName;
        }

        if (detailItemDescription != null)
        {
            string typeText = GetItemTypeText(item.type);
            detailItemDescription.text = $"類型: {typeText}\n數量: {item.quantity}\nID: {item.itemID}";
        }
    }

    string GetItemTypeText(InventoryItem.ItemType type)
    {
        switch (type)
        {
            case InventoryItem.ItemType.Key:
                return "鑰匙";
            case InventoryItem.ItemType.Consumable:
                return "消耗品";
            case InventoryItem.ItemType.Material:
                return "材料";
            case InventoryItem.ItemType.Quest:
                return "任務物品";
            default:
                return "未知";
        }
    }

    void ShowAllItems()
    {
        UpdateInventoryDisplay();
    }

    void FilterByType(InventoryItem.ItemType type)
    {
        currentFilter = type;
        UpdateInventoryDisplay();
    }

    public void ForceClose()
    {
        CloseInventoryPanel();
    }
}