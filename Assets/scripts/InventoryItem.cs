using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public string itemName;
    public Sprite itemIcon;
    public int quantity;
    public ItemType type;

    public enum ItemType
    {
        Key,        // 鑰匙
        Consumable, // 消耗品
        Material,   // 材料
        Quest       // 任務物品
    }

    public InventoryItem(string id, string name, Sprite icon, int qty, ItemType itemType)
    {
        itemID = id;
        itemName = name;
        itemIcon = icon;
        quantity = qty;
        type = itemType;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [Header("背包系統")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("音效")]
    public AudioClip pickupSound;
    private AudioSource audioSource;

    // 背包UI參考
    private InventoryUI inventoryUI;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryDisplay();
        }
    }

    // ========== 鑰匙系統 ==========
    public bool HasKey(string keyID)
    {
        foreach (var item in items)
        {
            if (item.itemID == keyID && item.type == InventoryItem.ItemType.Key)
            {
                return true;
            }
        }
        return false;
    }

    public void AddKey(string keyID, string keyName, Sprite icon = null)
    {
        // 檢查是否已有該鑰匙
        foreach (var item in items)
        {
            if (item.itemID == keyID)
            {
                item.quantity++;
                Debug.Log($"[背包] {keyName} 數量 +1，現有 {item.quantity} 個");
                PlayPickupSound();
                UpdateUI();
                return;
            }
        }

        // 新增鑰匙
        InventoryItem newKey = new InventoryItem(keyID, keyName, icon, 1, InventoryItem.ItemType.Key);
        items.Add(newKey);
        Debug.Log($"[背包] 獲得新鑰匙: {keyName}");
        PlayPickupSound();
        UpdateUI();
    }

    public void UseKey(string keyID)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemID == keyID && items[i].type == InventoryItem.ItemType.Key)
            {
                items[i].quantity--;
                Debug.Log($"[背包] 使用鑰匙: {items[i].itemName}，剩餘 {items[i].quantity} 個");

                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                    Debug.Log($"[背包] 鑰匙 {keyID} 已用完並移除");
                }

                UpdateUI();
                return;
            }
        }
    }

    // ========== 通用物品系統 ==========
    public void AddItem(string itemID, string itemName, Sprite icon, int quantity = 1, InventoryItem.ItemType type = InventoryItem.ItemType.Material)
    {
        // 檢查是否已有該物品
        foreach (var item in items)
        {
            if (item.itemID == itemID)
            {
                item.quantity += quantity;
                Debug.Log($"[背包] {itemName} 數量 +{quantity}，現有 {item.quantity} 個");
                PlayPickupSound();
                UpdateUI();
                return;
            }
        }

        // 新增物品
        InventoryItem newItem = new InventoryItem(itemID, itemName, icon, quantity, type);
        items.Add(newItem);
        Debug.Log($"[背包] 獲得新物品: {itemName} x{quantity}");
        PlayPickupSound();
        UpdateUI();
    }

    public bool HasItem(string itemID)
    {
        foreach (var item in items)
        {
            if (item.itemID == itemID && item.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemQuantity(string itemID)
    {
        foreach (var item in items)
        {
            if (item.itemID == itemID)
            {
                return item.quantity;
            }
        }
        return 0;
    }

    public void RemoveItem(string itemID, int quantity = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemID == itemID)
            {
                items[i].quantity -= quantity;
                Debug.Log($"[背包] 移除 {items[i].itemName} x{quantity}");

                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                }

                UpdateUI();
                return;
            }
        }
    }

    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("[背包] 背包已清空");
        UpdateUI();
    }

    public List<InventoryItem> GetAllItems()
    {
        return new List<InventoryItem>(items);
    }

    public List<InventoryItem> GetItemsByType(InventoryItem.ItemType type)
    {
        List<InventoryItem> result = new List<InventoryItem>();
        foreach (var item in items)
        {
            if (item.type == type)
            {
                result.Add(item);
            }
        }
        return result;
    }

    void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    void UpdateUI()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryDisplay();
        }
    }
}