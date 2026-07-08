using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriceCheck : MonoBehaviour
{
    public RepeatingNPC NPC;
    public void Proceed()
    {
        PriceManager.Instance.SetQuestComplete(NPC.GetQuestAssociate());
        NPC.Proceed();
        PriceManager.Instance.ProgressQuest();
    }
    public void OpenQuiz()
    {
        PriceManager.Instance.AccessQuiz(NPC.currentSoal, this);
    }
}

[System.Serializable]
public class RepeatingNPC
{
    public InteractableNPC interactableNPC;
    public List<QuestAcossiate> questAcossiate;
    public List<MiniGameSoal> Soals;
    public int currentQuest;
    public MiniGameSoal currentSoal;

    public void SetNPC()
    {
        currentQuest = 0;
        if (questAcossiate.Count < 0) return;
        interactableNPC.SetNewDialogue(questAcossiate[currentQuest].questRelated);
    }
    public void Proceed()
    {
        currentQuest += 1;
        if (questAcossiate.Count < 0) return;
        Mathf.Clamp(currentQuest, 0, questAcossiate.Count-1);
        interactableNPC.SetNewDialogue(questAcossiate[currentQuest].questRelated);
        currentSoal = Soals[currentQuest];
    }
    public string GetQuestAssociate()
    {
        if(questAcossiate.Count < 0)return "";
        string quest = questAcossiate[currentQuest].questRelated;
        return quest;
    }
    public MiniGameSoal GetCurrentSoal()
    {
        if(Soals.Count < 0 )return null;
        MiniGameSoal soal = Soals[currentQuest];
        return soal;
    }
}
[System.Serializable]
public class MiniGameSoal
{
    public string soal;
    public List<string> jawaban;
    public string kunci;
}
[System.Serializable]
public class QuestAcossiate
{
    public string questRelated;
    public string itemRelated;
}