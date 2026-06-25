using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GarbageCollector : MonoBehaviour
{
    public static GarbageCollector Instance;
    public GameObject triggerStartQuest;
    public GameObject triggerEndQuest;
    public Vector2 amountToSpawn = new Vector2(10,15);
    public Spawner spawner;
    public string questName;
    public int rewardAmount;
    public int repeatableRewradAmount;
    int currentTotalSpawn;
    int currentTotalCollected;

    void Awake()
    {
        Instance = this;
    }
    public void startSpawn()
    {
        currentTotalSpawn = (int)Random.Range(amountToSpawn.x, amountToSpawn.y);
        currentTotalCollected = 0;
        spawner.PoolObejct(currentTotalSpawn);
        triggerStartQuest.SetActive(false);
        triggerEndQuest.SetActive(false);
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,$"Collected Garbage {currentTotalCollected} / {currentTotalCollected}");
        }
    }
    public void Collect()
    {
        currentTotalCollected+=1;
        if(currentTotalCollected >= currentTotalSpawn)
        {
            triggerEndQuest.SetActive(true);
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
            }
        }
        else
        {
            var questRelated = QuestSystem.instance.GetQuest(questName,true);
            if(questRelated != null)
            {
                QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,$"Collected Garbage {currentTotalCollected} / {currentTotalCollected}");                
            }
        }
    }
    public void OpenAgain()
    {
        if(currentTotalCollected >= currentTotalSpawn)
        {
            triggerStartQuest.SetActive(true);
            triggerEndQuest.SetActive(false);
        }
    }
}
