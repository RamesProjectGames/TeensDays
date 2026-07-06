using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
//using static UnityEditor.Progress;

public class ListItem : MonoBehaviour
{
    public ShopItemList ShopItemList;
    public ShopManager shopManager;

    public IEnumerator LoadShopItems()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");

        string jsonData = "";

        // 🔥 Handle Android (StreamingAssets dalam APK)
        if (path.Contains("://") || path.Contains("jar:"))
        {
            UnityWebRequest request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Gagal load JSON: " + request.error);
                yield break;
            }

            jsonData = request.downloadHandler.text;
        }
        else
        {
            // ✅ PC / Editor
            if (File.Exists(path))
            {
                jsonData = File.ReadAllText(path);
            }
            else
            {
                Debug.LogError("File tidak ditemukan: " + path);
                yield break;
            }
        }

        // 🔥 Parse JSON
        string wrappedJson = "{ \"items\": " + jsonData + "}";
        ShopItemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);
        shopManager.currentItemList = ShopItemList;

        // 🔥 Sort items by rarity (legend > epic > rare > common)
        var rarityOrder = new Dictionary<string, int>
        {
            { "legendary", 0 },
            { "epic", 1 },
            { "rare", 2 },
            { "common", 3 }
        };

        ShopItemList.items = ShopItemList.items
            .OrderBy(item => rarityOrder.ContainsKey(item.rarity.ToLower()) ? rarityOrder[item.rarity.ToLower()] : 999)
            .ToArray();

        // 🔥 Tampilkan ke UI
        for (int i = 0; i < ShopItemList.items.Length && i < shopManager.itemCards.Length; i++)
        {
            Sprite iconSprite = Resources.Load<Sprite>("ShopIcons/" + ShopItemList.items[i].icon);

            shopManager.itemCards[i].SetItem(
                ShopItemList.items[i].itemId,
                ShopItemList.items[i].name,
                ShopItemList.items[i].description,
                ShopItemList.items[i].price,
                ShopItemList.items[i].priceMoney,
                GameManager.Instance.playerData.ownedItems.Contains(ShopItemList.items[i].itemId),
                ShopItemList.items[i].isDiamondPayment,
                ShopItemList.items[i].rarity,
                iconSprite);
        }
        if(InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager instance is not available.");
            yield break;
        }
        var inventoryUIManager = InventoryManager.Instance.inventoryUIManager;
        if(inventoryUIManager == null)
        {
            Debug.LogWarning("InventoryUIManager is not assigned in InventoryManager.");
            yield break;
        }
        for(int i=0; i < ShopItemList.items.Length && i < inventoryUIManager.items.Length; i++)
        {
            Sprite iconSprite = Resources.Load<Sprite>("ShopIcons/" + ShopItemList.items[i].icon);
            inventoryUIManager.items[i].Setup(
                ShopItemList.items[i].itemId,
                ShopItemList.items[i].rarity,
                iconSprite,
                ShopItemList.items[i].name,
                inventoryUIManager.GetRaritySprite(ShopItemList.items[i].rarity));
        }        
    }
}




