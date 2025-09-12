using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance;
    public List<Quest> quests = new List<Quest>();
    public List<Quest> sideQuests = new List<Quest>(); // Side quest list
    [SerializeField] private int currentQuestIndex = 0;
    [SerializeField] private float blinkSpeed = 2f;

    public QuestUIManager questUIManager;

    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            // contoh: set parent0 sub0 done
            MarkQuestDone(0, 0, true, false);
            Debug.Log("Test masuk");
        }

        UpdateQuestDisplay();
        CheckAutoCompleteQuests();
        UpdateQuestOutlines();
    }

    public bool HasQuest(Quest questData)
    {
        return quests.Contains(questData);
    }

    public void UpdateQuestDisplay()
    {
        foreach (var quest in quests)
        {
            UpdateSingleQuestDisplay(quest);
        }
    }

    private void UpdateSingleQuestDisplay(Quest quest)
    {
        if (quest.questText == null) return;
        quest.questText.text = quest.isDone ? $"<s>{quest.text}</s>" : quest.text;

        // Update semua subquest kalau ada
        foreach (var sub in quest.subQuests)
        {
            if (sub.questText == null) continue;
            sub.questText.text = sub.isDone ? $"<s>{sub.text}</s>" : sub.text;
        }
    }

    public void MarkQuestDone(int parentIndex, int questIndex, bool isSubQuest, bool isSideQuest = false)
    {
        if (isSideQuest)
        {
            // ✅ Side Quest langsung selesai
            if (questIndex >= 0 && questIndex < sideQuests.Count)
            {
                var sideQuest = sideQuests[questIndex];
                sideQuest.isDone = true;
                UpdateSingleQuestDisplay(sideQuest);

                CheckAutoCompleteQuests();

                Debug.Log($"✅ Side Quest {questIndex} selesai!");
            }
            return;
        }

        if (isSubQuest)
        {
            // ✅ Subquest selesai
            if (parentIndex >= 0 && parentIndex < quests.Count)
            {
                var parentQuest = quests[parentIndex];

                if (questIndex >= 0 && questIndex < parentQuest.subQuests.Count)
                {
                    var subQuest = parentQuest.subQuests[questIndex];
                    subQuest.isDone = true;
                    UpdateSingleQuestDisplay(subQuest);

                    Debug.Log($"✅ SubQuest {questIndex} dari MainQuest {parentIndex} selesai!");

                    // ✅ cek apakah semua subquest selesai
                    if (parentQuest.subQuests.All(sq => sq.isDone))
                    {
                        parentQuest.isDone = true;
                        UpdateSingleQuestDisplay(parentQuest);

                        Debug.Log($"🎉 MainQuest {parentIndex} selesai karena semua SubQuest sudah beres!");

                        CheckAutoCompleteQuests();
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠ SubQuest index {questIndex} tidak valid di MainQuest {parentIndex}");
                }
            }
        }
        else
        {
            // ✅ Main quest (tanpa subquest) → langsung selesai
            if (questIndex >= 0 && questIndex < quests.Count)
            {
                var mainQuest = quests[questIndex];

                if (mainQuest.subQuests.Count == 0)
                {
                    mainQuest.isDone = true;
                    UpdateSingleQuestDisplay(mainQuest);

                    Debug.Log($"✅ MainQuest {questIndex} selesai (tanpa subquest).");
                }
                else
                {
                    Debug.Log($"📌 MainQuest {questIndex} punya subquest, selesaikan dulu subquest-nya.");
                }
            }
        }
    }

    public void CheckAutoCompleteQuests()
    {
        if(currentQuestIndex < quests.Count)
        {
            var quest = quests[currentQuestIndex];
            Debug.Log($"[CheckAutoComplete] Cek quest {currentQuestIndex}: {quest.text}, isDone={quest.isDone}");
             
            foreach (var sq in quest.subQuests)
            {
                Debug.Log($"   Subquest '{sq.text}' -> isDone={sq.isDone}");
            }

            if (quest.isDone && quest.subQuests.All(sq => sq.isDone))
            {
                Debug.Log("✅ Semua subquest selesai, quest utama done!");
                quest.isDone = true;
                currentQuestIndex++;
                ActivateQuestObject(currentQuestIndex);
            }
        }
        else
        {
            Debug.Log($"[CheckAutoComplete] currentQuestIndex {currentQuestIndex} out of range (total={quests.Count})");
        }
    }

    public void ActivateQuestObject(int index)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].questUIObject != null)
                quests[i].questUIObject.SetActive(i == index);
        }
    }

    private void UpdateQuestOutlines()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            var outline = quests[i].questOutline;
            if (outline == null) continue;

            if (i == currentQuestIndex)
            {
                outline.enabled = true;

                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

                Color c = outline.OutlineColor;
                c.a = alpha;
                outline.OutlineColor = c;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    public void AddNewQuest(Quest questData, bool isMainQuest, bool isSubquest = false, bool isSideQuest = false)
    {
        GameObject newItem = Instantiate(questUIManager.questItemPrefab);

        if (isMainQuest)
        {
            newItem.transform.SetParent(questUIManager.panelMainQuestList, false);

        }
        else
        {
            newItem.transform.SetParent(questUIManager.panelSubQuestList, false);
        }

        TMP_Text questText = newItem.GetComponentInChildren<TMP_Text>();
        if (isMainQuest && !isSubquest)
        {
            questText.color = Color.yellow;
        }
        else if (!isMainQuest && isSubquest)
        {
            Color customBlue;
            if (ColorUtility.TryParseHtmlString("#00EEFF", out customBlue))
            {
                questText.color = customBlue;
            }
        }

        // Judul quest
        TMP_Text mainText = newItem.GetComponentInChildren<TMP_Text>();
        mainText.text = questData.text;

        // Simpan referensi
        questData.questUIObject = newItem;
        questData.questText = mainText;
        questData.questOutline = newItem.GetComponent<Outline>();

        // Spawn subquest
        Transform subQuestParent = newItem.transform.Find("Content");

        if (subQuestParent == null)
        {
            Debug.LogError("Parent untuk subquest tidak ditemukan di prefab! Pastikan ada child bernama Content");
        }

        foreach (var sub in questData.subQuests)
        {
            Debug.Log("Subquest masuk");

            if (sub.questUIObject != null) continue;

            GameObject subItem = Instantiate(questUIManager.subQuestItemPrefab, subQuestParent);
            TMP_Text subText = subItem.GetComponentInChildren<TMP_Text>();
            subText.text = sub.text;

            // simpan referensi subquest → supaya bisa di-update nanti
            sub.questUIObject = subItem;
            sub.questText = subText;
            sub.questOutline = subItem.GetComponent<Outline>();
        }
    }

}
