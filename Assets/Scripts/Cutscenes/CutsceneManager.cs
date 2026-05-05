using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CutsceneController;

public class CutsceneManager : MonoBehaviour
{
    public List<CutsceneController> cutscenes = new List<CutsceneController>();
    public CutsceneType cutsceneType;
    // Start is called before the first frame update
    void Awake()
    {        
        GameManager.Instance.onLoadDataComplete += StartCutscene;
        foreach (CutsceneController cutscene in cutscenes)
        {
            cutscene.gameObject.SetActive(false);
        }
    }

    public void StartCutscene()
    {
        int unlockedLevel = GameManager.Instance.playerData.unlockedLevel;
        
        if (unlockedLevel >= 7)
            cutsceneType = CutsceneType.Cutscene2;
        else
            cutsceneType = CutsceneType.Cutscene1;
        
        foreach (CutsceneController cutscene in cutscenes)
        {
            if (cutscene.cutsceneType == cutsceneType)
            {
                cutscene.gameObject.SetActive(true);
            }
        }
        
    }
}
