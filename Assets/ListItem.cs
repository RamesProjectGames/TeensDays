using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

public class ListItem : MonoBehaviour
{
    public ShopItemList ShopItemList;
    public ShopManager shopManager;

    private void Start()
    {

        LoadShopItems();
    }

    public void LoadShopItems()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");
        

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            string wrappedJson = "{ \"items\": " + jsonData + "}";

            // ✅ simpan data
            ShopItemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);
            shopManager.currentItemList = ShopItemList;


            // ✅ tampilkan ke UI
            for (int i = 0; i < ShopItemList.items.Length && i < shopManager.itemCards.Length; i++)
            {
                Sprite iconSprite = Resources.Load<Sprite>("ShopIcons/" + ShopItemList.items[i].icon);

                shopManager.itemCards[i].SetItem(
                    ShopItemList.items[i].itemId,
                    ShopItemList.items[i].name,
                    ShopItemList.items[i].description,
                    ShopItemList.items[i].price,
                    ShopItemList.items[i].priceMoney,
                    ShopItemList.items[i].isDiamondPayment,
                    ShopItemList.items[i].rarity,
                    iconSprite);


            }
        }
    }
}




