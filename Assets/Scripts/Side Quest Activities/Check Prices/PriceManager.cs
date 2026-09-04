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

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && IsActiveSideQuest())
        {
            RefreshQuestProgressDisplay();
        }
    }

    private bool IsActiveSideQuest()
    {
        if (QuestSystem.instance == null)
        {
            return false;
        }

        int activeSideQuestIndex = QuestSystem.instance.GetCurrentSideQuestIndex();
        return activeSideQuestIndex >= 0
            && activeSideQuestIndex < QuestSystem.instance.sideQuests.Count
            && QuestSystem.instance.sideQuests[activeSideQuestIndex].text == questName;
    }

    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true, 1);
        RefreshQuestProgressDisplay();
        relatedNPC.gameObject.SetActive(true);
        foreach (var vendor in vendors)
        {
            vendor.NPC.interactableNPC.gameObject.SetActive(false);
        }
        lastCheckIbu.NPC.interactableNPC.gameObject.SetActive(false);
        questTarget = relatedNPC != null ? relatedNPC.transform : null;
        SetQuestTarget(questTarget);
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        relatedNPC.gameObject.SetActive(false);
        
        QuizUI.SetActive(false);
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
        if(relatedSubQuest != null && !relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(5,subQuestIndex , true, true);
            QuestSystem.instance.CheckAutoCompleteQuests();
            TrackProgressFromSubQuests(questName, true, 1);
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
        MarkStarted();
        relatedNPC.gameObject.SetActive(false);
        foreach (var vendor in vendors)
        {
            vendor.NPC.SetNPC();
            vendor.NPC.interactableNPC.gameObject.SetActive(true);
        }
        RefreshQuestProgressDisplay();
        TrackProgressFromSubQuests(questName, true, 1);
        questTarget = vendors.Count > 0 && vendors[0].NPC != null && vendors[0].NPC.interactableNPC != null
            ? vendors[0].NPC.interactableNPC.transform
            : null;
        SetQuestTarget(questTarget);
    }
    private void RefreshQuestProgressDisplay()
    {
        string groceryToAdd = "Daftar Belanja : \n";
        int questCompletedAmount = 0;
        int totalAmount = 0;

        foreach (var vendor in vendors)
        {
            if (vendor == null || vendor.NPC == null)
            {
                continue;
            }

            foreach (var quest in vendor.NPC.questAcossiate)
            {
                if (string.IsNullOrEmpty(quest.itemRelated))
                {
                    continue;
                }

                totalAmount += 1;
                if (CheckQuestComplete(quest.questRelated))
                {
                    groceryToAdd += $"<color=#4CAF50><s>{quest.itemRelated}</s></color> \n";
                    questCompletedAmount += 1;
                }
                else
                {
                    groceryToAdd += $"<color=#FFFFFF>{quest.itemRelated}</color> \n";
                }
            }
        }

        if (totalAmount > 0)
        {
            SetProgress((float)questCompletedAmount / totalAmount);
        }

        var questRelated = QuestSystem.instance.GetQuest(questName, true);
        if (questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, groceryToAdd);
        }

        if (questCompletedAmount >= totalAmount && totalAmount > 0)
        {
            FinishQuest();
        }
    }
    private void SetQuestTarget(Transform target)
    {
        questTarget = target;
        if (QuestPathManager.Instance != null)
        {
            QuestPathManager.Instance.SetQuestTarget(target);
        }
    }
    public void ProgressQuest()
    {
        RefreshQuestProgressDisplay();
        if (vendors.Count > 0 && vendors[0].NPC != null && vendors[0].NPC.interactableNPC != null)
        {
            questTarget = vendors[0].NPC.interactableNPC.transform;
            SetQuestTarget(questTarget);
        }
    }
    public void FinishQuest()
    {
        if (lastCheckIbu != null && lastCheckIbu.NPC != null && lastCheckIbu.NPC.interactableNPC != null)
        {
            questTarget = lastCheckIbu.NPC.interactableNPC.transform;
            SetQuestTarget(questTarget);
        }
        relatedNPC.SetNewDialogue(lastCheck);
        relatedNPC.onTalkEnded.RemoveAllListeners();
        relatedNPC.onTalkEnded.AddListener(() =>
        {
            AccessLastQuiz();
        });
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,"<color=#4CAF50>Bantu Hitung Ibu total semua barang</color>");
        }
        if (lastCheckIbu != null && lastCheckIbu.NPC != null && lastCheckIbu.NPC.interactableNPC != null)
        {
            QuestPathManager.Instance.SetQuestTarget(lastCheckIbu.NPC.interactableNPC.transform);
        }
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
                        var questRelated = QuestSystem.instance.GetQuest(questName, true);
                        if (questRelated != null)
                        {
                            TrackProgressFromSubQuests(questName, true, 1);
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
                        }
                        if (questRelated != null && questRelated.isDone)
                        {
                            CompleteProgress();
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

