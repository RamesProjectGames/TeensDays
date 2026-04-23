using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeChanger : MonoBehaviour
{
    [System.Serializable]
    public class CostumePart
    {
        public string partName; // Head, Body, dll
        public SkinnedMeshRenderer targetRenderer;
        public SkinnedMeshRenderer bobonTargetSkin;

        public Mesh mesh;
        public Material material;
    }

    [System.Serializable]
    public class CostumeSet
    {
        public string costumeName;
        public CostumePart[] parts;
    }

    [Header("List Costume Set")]
    public CostumeSet[] costumeSets;

    private int currentIndex = 0;

    // 🔁 Next Kostum (dipanggil dari button)
    public void NextCostume()
    {
        currentIndex++;

        if (currentIndex >= costumeSets.Length)
            currentIndex = 0;

        ApplyCostume(costumeSets[currentIndex]);
    }

    // 🔙 Previous Kostum
    public void PreviousCostume()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = costumeSets.Length - 1;

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
