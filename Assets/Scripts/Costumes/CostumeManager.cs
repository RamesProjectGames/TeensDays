using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CostumeManager : MonoBehaviour
{
    public static CostumeManager Instance;
    public List<CostumeSet> playerCostumeSets;
    public List<CostumeSet> shopCostumeSets;
    public List<CostumeSet> profileCostumeSets;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    public void LoadCurrentSkin()
    {
        ApplyCostume(GameManager.Instance.playerData.currentSkinId);
        ApplyProfileCostume(GameManager.Instance.playerData.currentSkinId);
        ApplyPreviewCostume(GameManager.Instance.playerData.currentSkinId);
    }
    public void ApplyPreviewCostume(string skinId)
    {
        foreach (var set in shopCostumeSets)
        {
            if (set.itemId == skinId)
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(true)); // Aktifkan GameObject untuk bagian kostum ini
            }
            else
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(false)); // Nonaktifkan GameObject untuk bagian kostum ini
            }
        }
    }
    public void ApplyCostume(string skinId)
    {
        if(!GameManager.Instance.playerData.ownedItems.Contains(skinId))
        {
            Debug.LogWarning("Player does not own the costume with skinId: " + skinId);
            return;
        }
        if(GameManager.Instance.playerData.currentSkinId != skinId)
        {
            GameManager.Instance.playerData.currentSkinId = skinId;
        }
        foreach (var set in playerCostumeSets)
        {
            if (set.itemId == skinId)
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(true)); // Aktifkan GameObject untuk bagian kostum ini
            }
            else
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(false)); // Nonaktifkan GameObject untuk bagian kostum ini
            }
        }
        GameManager.Instance.SavePlayerDataToCloud(); // Save the current skin ID to cloud
    }
    public void ApplyProfileCostume(string skinId)
    {
        if(!GameManager.Instance.playerData.ownedItems.Contains(skinId))
        {
            Debug.LogWarning("Player does not own the costume with skinId: " + skinId);
            return;
        }
        if(GameManager.Instance.playerData.currentSkinId != skinId)
        {
            GameManager.Instance.playerData.currentSkinId = skinId;
        }
        foreach (var set in profileCostumeSets)
        {
            if (set.itemId == skinId)
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(true)); // Aktifkan GameObject untuk bagian kostum ini
            }
            else
            {
                set.parts.targetObjects.ForEach(part => part.SetActive(false)); // Nonaktifkan GameObject untuk bagian kostum ini
            }
        }
        ApplyCostume(skinId); // Apply the costume to the player as well
    }
}
