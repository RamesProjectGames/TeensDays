using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask npcLayer;
    public GameObject floatingButton;
    public GameObject chatPanel;
    public TMP_Text chatText;

    private InteractableNPC currentNPC;
    public QuestSystem questSystem;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, npcLayer);

        if (hits.Length > 0)
        {
            currentNPC = hits[0].GetComponent<InteractableNPC>();
            if (currentNPC != null)
                floatingButton.SetActive(true);
        }
        else
        {
            currentNPC = null;
            floatingButton.SetActive(false);
        }
    }

    public void OnTalkButtonClicked()
    {
        if (currentNPC != null)
        {
            var npcData = DialogManager.Instance.GetDialogByID(currentNPC.npcId);

            if (npcData != null)
            {
                chatPanel.SetActive(true);
                chatText.text = $"{npcData.name}: {npcData.dialog[0]}";

                // kalau NPC ini punya quest
                if (npcData.givesQuest)
                {
                    if (npcData.isSubQuest)
                    {
                        // Subquest
                        QuestSystem.instance.AddNewQuest(
                            QuestSystem.instance.quests[npcData.parentIndex].subQuests[npcData.questIndex],
                            false,   // isMainQuest
                            true,    // isSubQuest
                            false    // isSideQuest
                        );
                    }
                    else if (npcData.isSideQuest)
                    {
                        // Side quest
                        QuestSystem.instance.AddNewQuest(
                            QuestSystem.instance.sideQuests[npcData.questIndex],
                            false,   // isMainQuest
                            false,   // isSubQuest
                            true     // isSideQuest
                        );
                    }
                    else if (npcData.isMainQuest)
                    {
                        // Main quest
                        QuestSystem.instance.AddNewQuest(
                            QuestSystem.instance.quests[npcData.questIndex],
                            true,    // isMainQuest
                            false,   // isSubQuest
                            false    // isSideQuest
                        );
                    }
                }
            }
            else
            {
                Debug.LogWarning("NPC dialog not found for id: " + currentNPC.npcId);
            }
        }
    }

    public void OnQuestButtonClicked()
    {
        if (currentNPC == null) return;

        var npcData = DialogManager.Instance.GetDialogByID(currentNPC.npcId);
        if (npcData == null) return;

        if (npcData.givesQuest)
        {
            Debug.Log($"▶ NPC {npcData.name}, parentIndex={npcData.parentIndex}, questIndex={npcData.questIndex}");

            if (npcData.isSubQuest)
            {
                // ✅ Kalau ini SubQuest → langsung selesai
                QuestSystem.instance.MarkQuestDone(npcData.parentIndex, npcData.questIndex, true, false);
                Debug.Log($"✅ Subquest {npcData.questIndex} dari Quest {npcData.parentIndex} selesai!");
            }
            else if (npcData.isMainQuest)
            {
                var quest = QuestSystem.instance.quests[npcData.questIndex];

                if (quest.subQuests.Count > 0)
                {
                    // ✅ MainQuest punya SubQuest → otomatis mark salah satu subquest
                    int subIndex = quest.subQuests.FindIndex(sq => !sq.isDone);
                    if (subIndex >= 0)
                    {
                        QuestSystem.instance.MarkQuestDone(npcData.questIndex, subIndex, true, false);
                        Debug.Log($"📌 Main Quest {npcData.questIndex}: Subquest {subIndex} selesai!");
                    }
                    else
                    {
                        Debug.Log($"🎉 Semua Subquest dari MainQuest {npcData.questIndex} sudah selesai → MainQuest auto selesai!");
                    }
                }
                else
                {
                    // ✅ Kalau tidak punya subquest → langsung selesai
                    QuestSystem.instance.MarkQuestDone(-1, npcData.questIndex, false, false);
                    Debug.Log($"✅ Main Quest {npcData.questIndex} selesai (tanpa subquest).");
                }
            }
            else if (npcData.isSideQuest)
            {
                var sideQuest = QuestSystem.instance.sideQuests[npcData.questIndex];

                // Cek apakah side quest punya subquest
                if (sideQuest.subQuests != null && sideQuest.subQuests.Count > 0)
                {
                    // ✅ Mark subquest tertentu done
                    QuestSystem.instance.MarkQuestDone(npcData.questIndex, npcData.subQuestIndex, true, true);

                    // ✅ Cek apakah semua subquest sudah selesai
                    bool allDone = sideQuest.subQuests.All(sq => sq.isDone);
                    if (allDone && !sideQuest.isDone)
                    {
                        sideQuest.isDone = true;
                        QuestSystem.instance.UpdateSingleQuestDisplay(sideQuest);
                        Debug.Log($"✅ Semua subquest selesai → Side Quest {npcData.questIndex} selesai!");
                    }
                }
                else
                {
                    // Tidak punya subquest → langsung done
                    QuestSystem.instance.MarkQuestDone(-1, npcData.questIndex, false, true);
                    Debug.Log($"✅ Side Quest {npcData.questIndex} selesai!");
                }
            }
        }
        else
        {
            Debug.Log($"💬 NPC {npcData.name} hanya ngobrol, tidak ada quest.");
        }

        chatPanel.SetActive(false);
        floatingButton.SetActive(false);
    }
}
