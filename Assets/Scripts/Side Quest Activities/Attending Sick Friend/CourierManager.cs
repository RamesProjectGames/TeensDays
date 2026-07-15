using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourierManager : AssignmentManager
{
    public static CourierManager Instance;
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public InteractableNPC NPCRelated;
    public InteractableNPC EndNPC;
    void Awake()
    {
        Instance = this;
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        NPCRelated.gameObject.SetActive(true);
        EndNPC.gameObject.SetActive(false);
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        NPCRelated.gameObject.SetActive(false);
        EndNPC.gameObject.SetActive(true);
        EndNPC.onTalkEnded.RemoveAllListeners();
    }
    public void StartQuest()
    {
        NPCRelated.gameObject.SetActive(false);
        EndNPC.gameObject.SetActive(true);
        EndNPC.onTalkEnded.RemoveAllListeners();
        EndNPC.onTalkEnded.AddListener(CompleteQuest);
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, inCompleteDialogue, true);
        if(relatedSubQuest != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(relatedSubQuest, false, "Antarkan lembar materinya ke rumah teman.");
        }
        QuestPathManager.Instance.SetQuestTarget(EndNPC.transform);
    }
    public void CompleteQuest()
    {
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, completedDialogue, true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, completedDialogue, true);
        if(!relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(5,subQuestIndex , true, true);
            QuestSystem.instance.CheckAutoCompleteQuests();            
        }
        if(relatedSubQuest != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(relatedSubQuest, false, "");
        }
        QuestPathManager.Instance.SetQuestTarget(null);
        EndNPC.gameObject.SetActive(false);
        EndNPC.onTalkEnded.RemoveAllListeners();
    }
}
