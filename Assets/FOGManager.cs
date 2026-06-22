using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class UnlockedBuilding
{    
    public int unlockedClassMin, unlockedClassMax;
    public GameObject fogParticle;
    public GameObject Building;
    public GameObject Construction;
}
public class FOGManager : MonoBehaviour
{
    public static FOGManager Instance;
    [Header("Cameras")]
    public CinemachineFreeLook freeLookCam;
    public CinemachineVirtualCamera cinematicCam;
    public List<UnlockedBuilding> buildings = new List<UnlockedBuilding>();
    void Awake()
    {        
        Instance = this;
    }
    public void LoadBuilding()
    {
        var unlockedClass = GameManager.Instance.playerData.unlockedLevel;
        foreach (var building in buildings)
        {
            StartCoroutine(
                ShowObjectAfterDelay(
                    unlockedClass,
                    building.unlockedClassMin,
                    building.unlockedClassMax,
                    building.fogParticle,
                    building.Building,
                    building.Construction));
        }
    }
    private IEnumerator ShowObjectAfterDelay(int unlockedClass, int minClass, int MaxClass,GameObject fog,GameObject building, GameObject construction)
    {
        var showFog = false;

        if (IsBetween(unlockedClass, minClass , MaxClass) && !GameManager.Instance.playerData.unlockedSMP)
        {
            GameManager.Instance.playerData.unlockedSMP = true;
            showFog = true;
        }

        if (IsBetween(unlockedClass, minClass , MaxClass)&& !GameManager.Instance.playerData.unlockedSMA)
        {
            GameManager.Instance.playerData.unlockedSMA = true;
            showFog = true;
        }

        if (showFog)
        {
            fog.SetActive(true);
            // Pindah ke cinematic camera
            cinematicCam.Priority = 20;
            freeLookCam.Priority = 10;
            cinematicCam.LookAt = building.transform;

            yield return new WaitForSeconds(5f);
        }

        building.SetActive(true);
        construction.SetActive(false);

        if(showFog)
        {
            yield return new WaitForSeconds(10f);

            cinematicCam.Priority = 10;
            freeLookCam.Priority = 20;
            fog.SetActive(false);            
        }
        
        yield return new WaitForSeconds(1f);
        if (unlockedClass >= 7 && !GameManager.Instance.playerData.unlockedSMP)
        {
            NavMeshManager.Instance.RebuildSurfaceOnCertainArea("SMP");
        }

        if (unlockedClass >= 10 && !GameManager.Instance.playerData.unlockedSMA)
        {
            NavMeshManager.Instance.RebuildSurfaceOnCertainArea("SMA");
        }
        GameManager.Instance.SavePlayerDataToCloud();
    }
    public static bool IsBetween(int value, int min, int max)
    {
        return value >= min && value <= max;
    }
}
