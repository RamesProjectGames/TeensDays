using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<string> ownedItems = new List<string>();
    public InventoryUIManager inventoryUIManager;

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
            GameManager.Instance.playerData.ownedItems = ownedItems.Distinct().ToList();
            GameManager.Instance.SavePlayerDataToCloud(); 
            inventoryUIManager.RefreshAll();
        }
    }

    public bool HasItem(string itemId)
    {
        return ownedItems.Contains(itemId);
    }

    public void LoadInventory()
    {
        // ownedItems.Clear();

        // contoh load sederhana (kalau item list sudah ada)
        if (ShopManager.instance == null || ShopManager.instance.currentItemList == null) return;
        foreach (var item in ShopManager.instance.currentItemList.items)
        {
            if (GameManager.Instance.playerData.ownedItems.Contains(item.itemId) && !ownedItems.Contains(item.itemId))
            {
                ownedItems.Add(item.itemId);
            }
        }
    }
    [ContextMenu("Unlock All Items")]
    public void UnlockAllItems()
    {
        foreach (var item in ShopManager.instance.currentItemList.items)
        {
            if (!ownedItems.Contains(item.itemId))
            {
                ownedItems.Add(item.itemId);
            }
        }
        GameManager.Instance.playerData.ownedItems = ownedItems.Distinct().ToList();
        GameManager.Instance.SavePlayerDataToCloud(); 
        inventoryUIManager.RefreshAll();
    }
}
