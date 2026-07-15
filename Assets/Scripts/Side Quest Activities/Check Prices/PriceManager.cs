using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceManager : AssignmentManager
{
    public static PriceManager Instance;
    
    public string questName;
    public string retryGatherID;
    public string lastCheck;
    public string checkRightAnswer, checkWrongAnswer;
    public Transform questTarget;
    public InteractableNPC relatedNPC;
    public PriceCheck lastCheckIbu;
    public List<PriceCheck> vendors = new List<PriceCheck>();
    public GameObject QuizUI;
    public TMP_Text questionText;
    public List<Button> answers = new List<Button>();

    void Awake()
    {
        Instance= this;
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        relatedNPC.gameObject.SetActive(true);
        foreach (var vendor in vendors)
        {
            vendor.NPC.interactableNPC.gameObject.SetActive(false);
        }
        lastCheckIbu.NPC.interactableNPC.gameObject.SetActive(false);
        QuestPathManager.Instance.SetQuestTarget(relatedNPC.transform);
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        relatedNPC.gameObject.SetActive(false);
        foreach (var vendor in vendors)
        {
            vendor.NPC.interactableNPC.gameObject.SetActive(false);
        }
        lastCheckIbu.NPC.interactableNPC.gameObject.SetActive(false);
    }
    public void SetQuestComplete(string questID)
    {
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, questID, true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, questID, true);
        if(!relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(5,subQuestIndex , true, true);
            QuestSystem.instance.CheckAutoCompleteQuests();            
        }
    }
    public bool CheckQuestComplete(string questID)
    {
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, questID, true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, questID, true);
        if(subQuestIndex >0)
        {
            return relatedSubQuest.isDone;
        }
        return false;
    }
    public void RetryGather()
    {
        var playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        if(playerInteraction == null) return;
        playerInteraction.StartDialog(retryGatherID);
        QuizUI.SetActive(false);
    }
    public void StartQuest()
    {
        relatedNPC.gameObject.SetActive(false);
        string groceryToAdd = "Daftar Belanja : \n";
        foreach (var vendor in vendors)
        {
            vendor.NPC.SetNPC();
            vendor.NPC.interactableNPC.gameObject.SetActive(true);
            foreach (var quest in vendor.NPC.questAcossiate)
            {
                groceryToAdd += $"{quest.itemRelated} \n";                
            }
        }
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,groceryToAdd);
        }
        QuestPathManager.Instance.SetQuestTarget(vendors[0].NPC.interactableNPC.transform);
    }
    public void ProgressQuest()
    {
        string groceryToAdd = "Daftar Belanja : \n";
        var questCompletedAmount = 0;
        var totalAmount = 0;
        foreach (var vendor in vendors)
        {
            foreach (var quest in vendor.NPC.questAcossiate)
            {
                if (CheckQuestComplete(quest.questRelated))
                {
                    groceryToAdd += $"<s>{quest.itemRelated}</s> \n";
                    questCompletedAmount += 1;
                }
                else
                {
                    groceryToAdd += $"{quest.itemRelated} \n";
                }                
                totalAmount+=1;
            }
        }
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,groceryToAdd);
        }

        if(questCompletedAmount >= totalAmount)
        {
            FinishQuest();
        }
    }
    public void FinishQuest()
    {
        relatedNPC.SetNewDialogue(lastCheck);
        relatedNPC.onTalkEnded.RemoveAllListeners();
        relatedNPC.onTalkEnded.AddListener(() =>
        {
            AccessLastQuiz();
        });
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,"Bantu Hitung Ibu total semua barang");
        }
        QuestPathManager.Instance.SetQuestTarget(lastCheckIbu.NPC.interactableNPC.transform);
    }
    public void AccessQuiz(MiniGameSoal relatedData = null, PriceCheck priceCheck = null)
    {
        if(relatedData == null)
        {
            QuizUI.SetActive(false);
            return;
        }
        QuestPathManager.Instance.SetQuestTarget(null);
        QuizUI.SetActive(true);
        questionText.text = relatedData.soal;
        List<string> jawaban = new List<string>();
        foreach (var jawab in relatedData.jawaban)
        {
            jawaban.Add(jawab);
        }
        Shuffle(jawaban);
        for (int i = 0; i < answers.Count; i++)
        {
            answers[i].onClick.RemoveAllListeners();
            answers[i].GetComponentInChildren<TextMeshProUGUI>().text = jawaban[i];
            answers[i].onClick.AddListener(() =>
            {
                if(priceCheck!=null)
                {
                    if (jawaban[i] == relatedData.kunci.ToString())
                    {
                        priceCheck.Proceed();
                    }
                    else
                    {
                        RetryGather();
                    }
                }
            });
        }
    }
    public void AccessLastQuiz()
    {
        if(lastCheckIbu.NPC.currentSoal == null)
        {
            QuizUI.SetActive(false);
            return;
        }
        QuestPathManager.Instance.SetQuestTarget(null);
        QuizUI.SetActive(true);
        questionText.text = lastCheckIbu.NPC.currentSoal.soal;
        List<string> jawaban = new List<string>();
        foreach (var jawab in lastCheckIbu.NPC.currentSoal.jawaban)
        {
            jawaban.Add(jawab);
        }
        Shuffle(jawaban);
        for (int i = 0; i < answers.Count; i++)
        {
            answers[i].onClick.RemoveAllListeners();
            answers[i].GetComponentInChildren<TextMeshProUGUI>().text = jawaban[i];
            answers[i].onClick.AddListener(() =>
            {
                if(lastCheckIbu!=null)
                {
                    if (jawaban[i] == lastCheckIbu.NPC.currentSoal.kunci.ToString())
                    {
                        relatedNPC.SetNewDialogue(checkRightAnswer);
                        SetQuestComplete(lastCheck);
                        var questRelated = QuestSystem.instance.GetQuest(questName,true);
                        if(questRelated != null)
                        {
                            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,"");
                            foreach (var reward in questRelated.questRewards)
                            {
                                GameManager.Instance.playerData.currMoney += reward.rewardAmount;
                            }
                        }
                    }
                    else
                    {
                        var playerInteraction = FindAnyObjectByType<PlayerInteraction>();
                        if (playerInteraction == null) return;
                        playerInteraction.StartDialog(checkWrongAnswer);
                        QuizUI.SetActive(false);
                    }
                }
            });
        }
    }
    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (list[i], list[random]) = (list[random], list[i]);
        }
    }
}

