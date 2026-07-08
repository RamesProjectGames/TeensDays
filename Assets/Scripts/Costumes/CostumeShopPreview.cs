using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CostumeChanger;

public class CostumeShopPreview : MonoBehaviour
{
    public string itemId;

    private void Awake()
    {
        // costumeDict = new Dictionary<string, CostumeSet>();

        // foreach (var set in costumeSets)
        // {
        //     costumeDict.Add(set.itemId, set);
        // }
    }

    // private CostumeSet previewSet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Ye Olde Colde

    // public CostumeSet costumeSets;

    // private int currentIndex = 0;

    // private Dictionary<string, CostumeSet> costumeDict;

    // public Button useButton;    

    //public void CostumePreview()
    //{
    //    ApplyCostume(costumeSets[currentIndex]);
    //}

    // void ApplyPreviewCostume(bool activate)
    // {
    //     foreach (var part in costumeSets.parts.targetObjects)
    //     {
    //         if (part == null)
    //             continue;

    //         // part.targetRenderer.sharedMesh = part.mesh;
    //         // part.targetRenderer.material = part.material;
            
    //         part.SetActive(activate); // Aktifkan GameObject untuk bagian kostum ini
    //     }
    // }

    // void ApplyCostume(bool activate)
    // {
    //     foreach (var part in costumeSets.parts.targetObjects)
    //     {
    //         if (part == null) continue;

    //         // part.targetRenderer.sharedMesh = part.mesh;
    //         // part.targetRenderer.material = part.material;
    //         // part.bobonTargetSkin.sharedMesh = part.mesh;
    //         // part.bobonTargetSkin.material = part.material;
    //         part.SetActive(activate); // Aktifkan GameObject untuk bagian kostum ini
    //     }
    #endregion
    public void ShowPreview(string itemId)
    {
        this.itemId = itemId;
        CostumeManager.Instance.ApplyPreviewCostume(itemId);
        // if (costumeDict.TryGetValue(itemId, out CostumeSet set))
        // {
        //     previewSet = set;

        //     ApplyPreviewCostume(set);
        // }
    }
    public void UseCostume(string itemId)
    {        
        this.itemId = itemId;
        CostumeManager.Instance.ApplyCostume(itemId);
    }
}
