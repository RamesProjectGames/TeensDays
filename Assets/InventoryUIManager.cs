using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public InventoryItemUI[] items;
    public List<Sprite> raritySprites; // List untuk menyimpan sprite rarity

    public void RefreshAll()
    {
        foreach (var item in items)
        {
            item.Refresh();
        }
    }
    public void ShowItem(string rarity)
    {
        foreach (var item in items)
        {
            if(rarity.ToLower() == "all")
            {
                item.gameObject.SetActive(true);
            }
            else if (item.rarity.ToLower().Contains(rarity.ToLower(), System.StringComparison.OrdinalIgnoreCase))
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }        
    }
    public Sprite GetRaritySprite(string rarity)
    {
        // Cari sprite berdasarkan rarity
        Sprite raritySprite = raritySprites.Find(sprite => sprite.name.ToLower() == rarity.ToLower());

        if (raritySprite != null)
        {
            return raritySprite;
        }
        else
        {
            Debug.LogWarning("Rarity sprite not found for: " + rarity);
            return null; // Atau kembalikan sprite default jika diinginkan
        }
    }
}
