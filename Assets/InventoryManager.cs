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

        // LoadInventory();
    }

    public void AddItem(string itemId)
    {
        if (!ownedItems.Contains(itemId))
        {
            ownedItems.Add(itemId);
            // PlayerPrefs.SetInt("ITEM_" + itemId, 1);
            // PlayerPrefs.Save();
            GameManager.Instance.playerData.ownedItems.list = ownedItems;
            GameManager.Instance.SavePlayerDataToCloud(); 
        }
    }

    public bool HasItem(string itemId)
    {
        return ownedItems.Contains(itemId);
    }

    public void LoadInventory()
    {
        ownedItems.Clear();

        // contoh load sederhana (kalau item list sudah ada)
        foreach (var item in ShopManager.instance.currentItemList.items)
        {
            if (GameManager.Instance.playerData.ownedItems.list.Contains(item.itemId))
            {
                ownedItems.Add(item.itemId);
            }
        }
    }
}
