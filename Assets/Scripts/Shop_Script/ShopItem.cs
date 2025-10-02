using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string itemId;
    public string name;
    public string description;
    public int price;
    public int priceMoney;
    public bool isDiamondPayment;
    public string rarity;
}

[System.Serializable]
public class ShopItemList
{
    public ShopItem[] items;
}
