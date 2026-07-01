using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CostumeChanger;

public class CostumeShopPreview : MonoBehaviour
{
    public string itemId;

    public Button useButton;

    private CostumeSet previewSet;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [System.Serializable]
    public class CostumePart
    {
        public string partName; // Head, Body, dll
        public SkinnedMeshRenderer targetRenderer;
        public SkinnedMeshRenderer bobonTargetSkin;

        public Mesh mesh;
        public Material material;
    }

    [Header("List Costume Set")]
    public CostumeSet[] costumeSets;

    private int currentIndex = 0;

    private Dictionary<string, CostumeSet> costumeDict;

    private void Awake()
    {
        costumeDict = new Dictionary<string, CostumeSet>();

        foreach (var set in costumeSets)
        {
            costumeDict.Add(set.itemId, set);
        }
    }

    public void ShowPreview(string itemId)
    {
        this.itemId = itemId;

        if (costumeDict.TryGetValue(itemId, out CostumeSet set))
        {
            previewSet = set;

            ApplyPreviewCostume(set);
        }
    }

    public void UseCostume()
    {
        PlayerPrefs.SetString("CurrentCostume", itemId);

        if (costumeDict.TryGetValue(itemId, out CostumeSet set))
        {
            ApplyCostume(set);
        }

        Debug.Log("Menggunakan costume " + itemId);
    }

    //public void CostumePreview()
    //{
    //    ApplyCostume(costumeSets[currentIndex]);
    //}

    void ApplyPreviewCostume(CostumeSet set)
    {
        foreach (var part in set.parts)
        {
            if (part.targetRenderer == null)
                continue;

            part.targetRenderer.sharedMesh = part.mesh;
            part.targetRenderer.material = part.material;
        }
    }

    void ApplyCostume(CostumeSet set)
    {
        foreach (var part in set.parts)
        {
            if (part.targetRenderer == null) continue;

            part.targetRenderer.sharedMesh = part.mesh;
            part.targetRenderer.material = part.material;
            part.bobonTargetSkin.sharedMesh = part.mesh;
            part.bobonTargetSkin.material = part.material;
        }

        Debug.Log("Ganti kostum ke: " + set.costumeName);
    }
}
