using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<string> ownedItems = new List<string>();

    private void Awake()
    {
        Instance = this;

        LoadInventory();
    }

    public void AddItem(string itemId)
    {
        if (!ownedItems.Contains(itemId))
        {
            ownedItems.Add(itemId);
            PlayerPrefs.SetInt("ITEM_" + itemId, 1);
            PlayerPrefs.Save();
        }
    }

    public bool HasItem(string itemId)
    {
        return ownedItems.Contains(itemId);
    }

    void LoadInventory()
    {
        ownedItems.Clear();

        // contoh load sederhana (kalau item list sudah ada)
        foreach (var item in ShopManager.instance.currentItemList.items)
        {
            if (PlayerPrefs.GetInt("ITEM_" + item.itemId, 0) == 1)
            {
                ownedItems.Add(item.itemId);
            }
        }
    }
}
