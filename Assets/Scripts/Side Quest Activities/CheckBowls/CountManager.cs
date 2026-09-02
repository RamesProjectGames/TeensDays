using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountManager : AssignmentManager
{
    public string questName;
    public string subQuestName, checkRightAnswer, checkWrongAnswer;
    public Transform questTarget;
    public InteractableNPC relatedNPC;
    public Spawner spawner;
    public GameObject QuizUI;
    public TMP_Text questionText;
    public List<Button> answers = new List<Button>();
    public MiniGameSoal relatedData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        relatedNPC.SetNewDialogue(questName);
        relatedNPC.onTalkEnded.RemoveAllListeners();
        relatedNPC.onTalkEnded.AddListener(StartQuest);
        QuestPathManager.Instance.SetQuestTarget(questTarget);
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        relatedNPC.gameObject.SetActive(false);        
        QuizUI.SetActive(false);
    }
    public void SetQuestComplete(string questID)
    {
        var relatedSubQuest = QuestSystem.instance.GetSubQuest(questName, questID, true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName, questID, true);
        if(relatedSubQuest != null && !relatedSubQuest.isDone && subQuestIndex > 0)
        {
            QuestSystem.instance.MarkQuestDone(6,subQuestIndex , true, true);
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
    public void StartQuest()
    {
        spawner.PoolObjects();
        SetQuestComplete(subQuestName);
        TrackProgressFromSubQuests(questName, true, 1);
        AccessQuiz();
    }
    public void FinishQuest()
    {
        SetQuestComplete(checkRightAnswer);
        SetQuestComplete(checkWrongAnswer);
        TrackProgressFromSubQuests(questName, true, 1);
        spawner.DeactivateAllObjects();
        var questRelated = QuestSystem.instance.GetQuest(questName, true);
        if (questRelated != null)
        {
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
        relatedNPC.gameObject.SetActive(false);
    }
    public void AccessQuiz()
    {
        if(relatedData == null)
        {
            QuizUI.SetActive(false);
            return;
        }
        QuestPathManager.Instance.SetQuestTarget(null);
        var questRelated = QuestSystem.instance.GetQuest(questName, true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, "Tell how much bowl on the table");
        }
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
                var questRelated = QuestSystem.instance.GetQuest(questName, true);
                if (jawaban[i] == relatedData.kunci.ToString())
                {
                    relatedNPC.SetNewDialogue(checkRightAnswer);
                    relatedNPC.onTalkEnded.RemoveAllListeners();
                    relatedNPC.onTalkEnded.AddListener(FinishQuest);
                }
                else
                {
                    relatedNPC.SetNewDialogue(checkWrongAnswer);
                    relatedNPC.onTalkEnded.RemoveAllListeners();
                    relatedNPC.onTalkEnded.AddListener(AccessQuiz);
                }
                if (questRelated != null)
                {
                    QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, "Talk to the vendor");
                }
                QuizUI.SetActive(false);
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
