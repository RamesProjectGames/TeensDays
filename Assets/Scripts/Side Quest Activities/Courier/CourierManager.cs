using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourierManager : AssignmentManager
{
    public static CourierManager Instance;
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public string subQuestText;
    public List<Transform> targetPosition;
    List<Transform> inCompleteTarget;
    List<Transform> completedTarget;
    public InteractableNPC NPCRelated;
    public InteractableNPC EndNPC;
    void Awake()
    {
        Instance = this;
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true, 1);
        if(NPCRelated == EndNPC)
        {
            NPCRelated.gameObject.SetActive(true);            
        }
        else
        {
            NPCRelated.gameObject.SetActive(true);
            EndNPC.gameObject.SetActive(false);            
        }
        inCompleteTarget.AddRange(targetPosition);
        completedTarget.Clear();
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        NPCRelated.gameObject.SetActive(false);
        EndNPC.gameObject.SetActive(false);
        EndNPC.onTalkEnded.RemoveAllListeners();
        inCompleteTarget.AddRange(targetPosition);
        completedTarget.Clear();
    }
    public void StartQuest()
    {
        MarkStarted();
        NPCRelated.gameObject.SetActive(false);
        NPCRelated.onTalkEnded.RemoveAllListeners();
        EndNPC.gameObject.SetActive(true);
        EndNPC.onTalkEnded.RemoveAllListeners();
        EndNPC.onTalkEnded.AddListener(CompleteQuest);
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, inCompleteDialogue, true);
        if(relatedSubQuest != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(relatedSubQuest, false, subQuestText);
        }
        int questIndex = QuestSystem.instance.GetQuestIndex(questName,true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, inCompleteDialogue, true);
        if(relatedSubQuest != null && !relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(questIndex,subQuestIndex , true, true);
            QuestSystem.instance.CheckAutoCompleteQuests();
        }
        TrackProgressFromSubQuests(questName, true, 1);
        if(targetPosition == null && targetPosition.Count >0)
        {
            QuestPathManager.Instance.SetQuestTarget(EndNPC.transform);            
        }
        else
        {
            foreach (var target in inCompleteTarget)
            {
                target.gameObject.SetActive(true);
            }
            QuestPathManager.Instance.SetQuestTarget(inCompleteTarget[0]);            
        }
    }
    public void ProgressQuest(Transform target)
    {
        if(NPCRelated == EndNPC)
        {
            MoveTransform(target,inCompleteTarget,completedTarget);
            foreach (var incompletetarget in inCompleteTarget)
            {
                incompletetarget.gameObject.SetActive(true);
            }
            foreach (var completetarget in completedTarget)
            {
                completetarget.gameObject.SetActive(false);
            }
            if(completedTarget.Count <=0)
            {
                NPCRelated.SetNewDialogue(completedDialogue);
                NPCRelated.gameObject.SetActive(true);
            }
            var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, inCompleteDialogue, true);
            if (relatedSubQuest != null)
            {
                QuestSystem.instance.UpdateCurrentQuestInfo(relatedSubQuest, false, target.GetComponent<Courier>()?.updateText);
            }
            SetProgress(Mathf.Clamp01((float)completedTarget.Count / (float) targetPosition.Count));
        }

    }
    public void CompleteQuest()
    {
        int questIndex = QuestSystem.instance.GetQuestIndex(questName,true);
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, completedDialogue, true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, completedDialogue, true);
        if(relatedSubQuest != null && !relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(questIndex,subQuestIndex , true, true);
            QuestSystem.instance.CheckAutoCompleteQuests();
        }
        if(relatedSubQuest != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(relatedSubQuest, false, "");
        }
        TrackProgressFromSubQuests(questName, true, 1);

        var questRelated = QuestSystem.instance.GetQuest(questName, true);
        if (questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, "");
            if (!questRelated.isDone)
            {
                foreach (var reward in questRelated.questRewards)
                {
                    if (reward.type == QuestRewardType.Money)
                    {
                        GameManager.Instance.playerData.currMoney += reward.rewardAmount;
                    }
                    else if (reward.type == QuestRewardType.Diamonds)
                    {
                        GameManager.Instance.playerData.currDiamond += reward.rewardAmount;
                    }
                }
            }
            else
            {
                foreach (var reward in questRelated.questRewards)
                {
                    if (reward.type == QuestRewardType.Money)
                    {
                        GameManager.Instance.playerData.currMoney += reward.rewardAmount / 10;
                    }
                    else if (reward.type == QuestRewardType.Diamonds)
                    {
                        GameManager.Instance.playerData.currDiamond += reward.rewardAmount / 10;
                    }
                }
            }

            if (questRelated.isDone)
            {
                CompleteProgress();
            }
        }
        QuestPathManager.Instance.SetQuestTarget(null);
        EndNPC.gameObject.SetActive(false);
        EndNPC.onTalkEnded.RemoveAllListeners();
    }
    void MoveTransform(Transform obj, List<Transform> fromList, List<Transform> toList)
    {
        if (fromList.Remove(obj))
        {
            toList.Add(obj);
        }
    }
}
