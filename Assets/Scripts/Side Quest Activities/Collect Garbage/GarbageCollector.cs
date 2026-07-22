using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GarbageCollector : AssignmentManager
{
    public static GarbageCollector Instance;
    public GameObject triggerStartQuest;
    public GameObject triggerEndQuest;
    public Vector2 amountToSpawn = new Vector2(10,15);
    public Spawner spawner;
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public int rewardAmount;
    public int repeatableRewradAmount;
    int currentTotalSpawn;
    int currentTotalCollected;

    void Awake()
    {
        Instance = this;
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true, 1);
        triggerStartQuest.SetActive(true);
        triggerEndQuest.SetActive(false);
        currentTotalCollected = 0;
        currentTotalSpawn = 0;
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        triggerStartQuest.SetActive(false);
        triggerEndQuest.SetActive(false);
        currentTotalCollected = 0;
        currentTotalSpawn = 0;
    }
    public void startSpawn()
    {
        MarkStarted();
        currentTotalSpawn = (int)Random.Range(amountToSpawn.x, amountToSpawn.y);
        currentTotalCollected = 0;
        spawner.PoolObejct(currentTotalSpawn);
        triggerStartQuest.SetActive(false);
        triggerEndQuest.SetActive(false);
        SetProgress(0f);
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,$"Collected Garbage {currentTotalCollected} / {currentTotalSpawn}");
        }
        QuestSystem.instance.MarkQuestDone(QuestSystem.instance.GetQuestIndex(questName,true),1,true,true);
    }
    public void Collect()
    {
        currentTotalCollected+=1;
        if (currentTotalSpawn > 0)
        {
            SetProgress((float)currentTotalCollected / currentTotalSpawn);
        }

        if(currentTotalCollected >= currentTotalSpawn)
        {
            triggerEndQuest.SetActive(true);
            CompleteProgress();
            var questRelated = QuestSystem.instance.GetQuest(questName,true);
            if(questRelated != null)
            {
                if(!questRelated.isDone)
                {
                    questRelated.isDone = true;
                    GameManager.Instance.playerData.currMoney += rewardAmount;
                    _ = QuestSystem.instance.SaveQuestsAsync();
                }
                else
                {
                    GameManager.Instance.playerData.currMoney += repeatableRewradAmount;
                }
                QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,$"Collected All Garbages");
                QuestSystem.instance.MarkQuestDone(QuestSystem.instance.GetQuestIndex(questName,true),1,true,true);
            }
        }
        else
        {
            var questRelated = QuestSystem.instance.GetQuest(questName,true);
            if(questRelated != null)
            {
                QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,$"Collected Garbage {currentTotalCollected} / {currentTotalSpawn}");                
            }
        }
    }
    public void OpenAgain()
    {
        if(currentTotalCollected >= currentTotalSpawn)
        {
            triggerStartQuest.SetActive(false);
            triggerEndQuest.SetActive(false);
            var questRelated = QuestSystem.instance.GetQuest(questName,true);
            if(questRelated != null)
            {
                QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,"");    
                QuestSystem.instance.RemoveQuestFromUI(questRelated);
            }
        }
    }
}
