using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CostumeChanger;

public class CostumeShopPreview : MonoBehaviour
{
    public string itemId;

    public Button useButton;
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

    public void CostumePreview()
    {
        ApplyCostume(costumeSets[currentIndex]);
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
