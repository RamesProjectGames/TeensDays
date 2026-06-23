using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroceriesManager : MonoBehaviour
{
    public static GroceriesManager Instance;
    public GameObject NPCRelated;
    public List<GameObject> groceriesLocations = new List<GameObject>();
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public int rewardAmount;
    public int repeatableRewradAmount;
    int currentGrocery;
    InteractableNPC interactable;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if(NPCRelated.TryGetComponent<InteractableNPC>(out var interactable))
        {
            this.interactable = interactable;
        }
        NPCRelated.SetActive(false);
        currentGrocery = 0;
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(false);
        }

    }
    public void ActivateQuest()
    {        
        NPCRelated.SetActive(true);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartQuest);
        }
    }
    public void StartQuest()
    {
        NPCRelated.SetActive(false);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
        }
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(i == currentGrocery);
        }
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,0,true);
    }
    public void ProgressQuest()
    {
        if(currentGrocery < groceriesLocations.Count)
        {
            currentGrocery+=1;
        }
        else
        {
            FinishQuest();
            return;
        }
        QuestPathManager.Instance.SetQuestTarget(groceriesLocations[currentGrocery].transform);
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(i== currentGrocery);
        }
    }
    public void FinishQuest()
    {
        NPCRelated.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.AddListener(() =>
            {
                QuestSystem.instance.MarkQuestDone(3, 1, true, true);
                var questRelated = QuestSystem.instance.GetQuest(questName, true);
                if (questRelated != null)
                {
                    if (!questRelated.isDone)
                    {
                        GameManager.Instance.playerData.currMoney += rewardAmount;
                    }
                    else
                    {
                        GameManager.Instance.playerData.currMoney += repeatableRewradAmount;
                    }
                }
                ResetQuest();
            });
        }        
        QuestSystem.instance.MarkQuestDone(3, 0, true, true);
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,1,true);
        
    }
    public void ResetQuest()
    {
        if(interactable != null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartQuest);
        }
    }
}
