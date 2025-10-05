using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public string description;
    public int price; // Inspector³]©w
    public Sprite icon;
    [System.NonSerialized]
    public IShopItemEffect effect;
}

