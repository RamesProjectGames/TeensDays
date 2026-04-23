using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineVirtualCamera talkCam;

    public float interactRange = 3f;
    public LayerMask npcLayer;
    public GameObject floatingButton;
    public GameObject chatPanel;
    private Quaternion originalRotation;
    public bool wasPatrolling = false;

    //public TMP_Text nameText;
    //public TMP_Text dialogText;

    //public GameObject questObjectAnnoun;
    // public GameObject questObjectSide;
    //public GameObject contohSideQuest;
    //public TMP_Text chatText;

    //public GameObject gerbangSekolah;

    public Image achieveAnnoun;
    public AudioSource achieveAnnounAudio;

    private InteractableNPC currentNPC;
    private InteractableNPC npcBeingTalkedTo;
    public QuestSystem questSystem;
    //public QuestPathManager questPathManager;
    public AchieveManager achieveManager;
    public PlayerManager playerManager;

    public GameObject[] imageAnnoun;

    [Header("Panel Bubble Chat")]
    private int dialogIndex = 0;
    private NPCDialogData activeDialog;
    public GameObject leftBubble;
    public GameObject rightBubble;
    public TMP_Text leftNameText;
    public TMP_Text leftDialogText;
    public TMP_Text rightNameText;
    public TMP_Text rightDialogText;
    public Image leftPortrait;
    public Image rightPortrait;
    public int questIndex;

    private void Start()
    {
        StartQuest();
    }

    void Update()
    {
        if (npcBeingTalkedTo != null) return; // <-- NPC locked, jangan ubah currentNPC

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
        //Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, npcLayer);

        //Debug.Log(hits.Length);

        //if (hits.Length > 0)
        //{
        //    currentNPC = hits[0].GetComponent<InteractableNPC>();
        //    if (currentNPC != null)
        //        floatingButton.SetActive(true);

        //}
        //else
        //{
        //    currentNPC = null;
        //    floatingButton.SetActive(false);
        //}
    }

    public void OnTalkButtonClicked()
    {
        if (currentNPC == null) return;

        npcBeingTalkedTo = currentNPC;  // <-- Kunci NPC saat ini

        // =======================
        // 🎥 Aktifkan kamera dialog
        // =======================
        Transform lookTarget = npcBeingTalkedTo.transform.Find("HeadLookTarget");
        if (lookTarget != null)
        {
            talkCam.LookAt = lookTarget;
        }
        else
        {
            Debug.Log("Tidak menemukan HeadLookTarget");
            talkCam.LookAt = npcBeingTalkedTo.transform;
        }

        talkCam.Priority = 20;

        var data = NPCPatrolManager.Instance.GetNPCData(npcBeingTalkedTo.gameObject);
        if (data != null)
        {
            data.isPaused = true;
            data.agent.isStopped = true;
        }

        // Simpan rotasi awal sebelum menghadap ke player
        originalRotation = currentNPC.transform.rotation;

        // Simpan status patrol sebelumnya
        NavMeshAgent agent = currentNPC.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            wasPatrolling = !agent.isStopped; // Jika agent sedang berjalan, berarti dia patrol
            agent.isStopped = true;           // Stop untuk dialog
        }

        // ==========================
        //  NPC Hadap ke Player
        // ==========================
        Vector3 dir = (transform.position - currentNPC.transform.position).normalized;
        dir.y = 0;
        currentNPC.transform.rotation = Quaternion.LookRotation(dir);

        //// ==========================
        ////  NPC Animasi Idle
        //// ==========================
        //Animator npcAnim = currentNPC.GetComponent<Animator>();
        //if (npcAnim != null)
        //{
        //    npcAnim.SetBool("isWalking", false);
        //    npcAnim.SetBool("isIdle", true);
        //    npcAnim.SetFloat("Speed", 0f);   // Kalau pakai blend tree
        //}

        if (currentNPC == null) return;

        activeDialog = DialogManager.Instance.GetDialogByID(currentNPC.npcId);
        if (activeDialog == null) return;

        dialogIndex = 0;
        chatPanel.SetActive(true);

        ShowDialogLine();
        #region old
        //if (currentNPC != null)
        //{
        //    var npcData = DialogManager.Instance.GetDialogByID(currentNPC.npcId);

        //    if (npcData != null)
        //    {
        //        chatPanel.SetActive(true);
        //        chatText.text = $"{npcData.name}: {npcData.dialog[0]}";

        //        // kalau NPC ini punya quest
        //        if (npcData.givesQuest)
        //        {
        //            if (npcData.isSubQuest)
        //            {
        //                // Subquest
        //                QuestSystem.instance.AddNewQuest(
        //                    QuestSystem.instance.quests[npcData.parentIndex].subQuests[npcData.questIndex],
        //                    false,   // isMainQuest
        //                    true,    // isSubQuest
        //                    false    // isSideQuest
        //                );
        //            }
        //            else if (npcData.isSideQuest)
        //            {
        //                // Side quest
        //                QuestSystem.instance.AddNewQuest(
        //                    QuestSystem.instance.sideQuests[npcData.questIndex],
        //                    false,   // isMainQuest
        //                    false,   // isSubQuest
        //                    true     // isSideQuest
        //                );
        //            }
        //            else if (npcData.isMainQuest)
        //            {
        //                // Main quest
        //                QuestSystem.instance.AddNewQuest(
        //                    QuestSystem.instance.quests[npcData.questIndex],
        //                    true,    // isMainQuest
        //                    false,   // isSubQuest
        //                    false    // isSideQuest
        //                );
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogWarning("NPC dialog not found for id: " + currentNPC.npcId);
        //    }
        //}
        #endregion
    }

    public void StartQuest()
    {
        // Pastikan QuestSystem sudah siap
        if (QuestSystem.instance == null) return;

        // Tambahkan quest utama pertama
        QuestSystem.instance.AddNewQuest(
            QuestSystem.instance.quests[questIndex],
            true,   // isMainQuest
            false,  // isSubQuest
            false   // isSideQuest
        );

        // Set target jika ada
        var quest = QuestSystem.instance.quests[questIndex];
        if (quest.targetTransform != null)
        {
            QuestSystem.instance.questPathManager
                .SetQuestTarget(quest.targetTransform);
        }

        Debug.Log("✅ Quest langsung aktif saat Start Game!");
    }

    void ShowDialogLine()
    {
        if (dialogIndex >= activeDialog.dialog.Count)
        {
            EndDialog();
            return;
        }

        var line = activeDialog.dialog[dialogIndex];

        // Matikan semua bubble dulu
        leftBubble.SetActive(false);
        rightBubble.SetActive(false);

        if (line.isPlayer)
        {
            // BUBBLE KANAN (PLAYER)
            rightBubble.SetActive(true);
            rightNameText.text = line.speaker;
            rightDialogText.text = line.text;
        }
        else
        {
            // BUBBLE KIRI (NPC)
            leftBubble.SetActive(true);
            leftNameText.text = line.speaker;
            leftDialogText.text = line.text;
        }

        dialogIndex++;
    }

    public void OnNextDialogClicked()
    {
        if (activeDialog == null) return;
        ShowDialogLine();
    }

    void EndDialog()
    {
        if (npcBeingTalkedTo != null)
        {
            var data = NPCPatrolManager.Instance.GetNPCData(npcBeingTalkedTo.gameObject);
            if (data != null)
            {
                data.isPaused = false;
                data.agent.isStopped = false;
            }
        }

        npcBeingTalkedTo = null;  // <— penting

        if (currentNPC != null)
        {
            currentNPC.transform.rotation = originalRotation;

            // ===============================
            // Kembalikan patrol NPC
            // ===============================
            NavMeshAgent agent = currentNPC.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                if (wasPatrolling)
                {
                    agent.isStopped = false; // lanjut patrol
                }
                else
                {
                    agent.isStopped = true;  // jika sebelumnya memang idle
                }
            }
        }

        chatPanel.SetActive(false);
        leftBubble.SetActive(false);
        rightBubble.SetActive(false);

        dialogIndex = 0;
        OnQuestButtonClicked();

        int startQuestIndex = activeDialog.questIndex;                   // Quest yang muncul saat Start Game
        int nextQuestIndex = startQuestIndex + 1; // Quest berikutnya
        if (nextQuestIndex < QuestSystem.instance.quests.Count)
        {
            var nextQuest = QuestSystem.instance.quests[nextQuestIndex];

            QuestSystem.instance.AddNewQuest(
                nextQuest,
                true,   // isMainQuest
                false,  // isSubQuest
                false   // isSideQuest
            );

            Debug.Log($"➡️ Quest berikutnya aktif: {nextQuest.text}");

            if (nextQuest.targetTransform != null)
            {
                QuestSystem.instance.questPathManager
                    .SetQuestTarget(nextQuest.targetTransform);

                Debug.Log($"🎯 Target diarahkan ke {nextQuest.targetTransform.name}");
            }
        }

        // =======================
        // 🎥 Matikan kamera dialog
        // =======================
        talkCam.Priority = 0;
        talkCam.LookAt = null;

        // ============================
        // ✅ QUEST START GAME AUTO SELESAI
        // ============================

        //// ✅ Selesaikan Quest Awal
        //var startQuest = QuestSystem.instance.quests[startQuestIndex];
        //startQuest.questObjectAnnoun?.SetActive(false);

        //QuestSystem.instance.MarkQuestDone(-1, startQuestIndex, false, false);

        //QuestSystem.instance.UpdateQuestDisplay();
        //QuestSystem.instance.CheckAutoCompleteQuests();

        //Debug.Log("✅ Quest awal otomatis diselesaikan di akhir dialog!");

        //// ============================
        //// ✅ AKTIFKAN QUEST SELANJUTNYA
        //// ============================


        //chatPanel.SetActive(false);
        //floatingButton.SetActive(false);
        //dialogIndex = 0;

        //if (activeDialog.givesQuest)
        //{
        //    GiveQuest(activeDialog);
        //}
    }

    void GiveQuest(NPCDialogData npcData)
    {
        if (npcData.isSubQuest)
        {
            QuestSystem.instance.AddNewQuest(
                QuestSystem.instance.quests[npcData.parentIndex].subQuests[npcData.questIndex],
                false, true, false
            );
        }
        else if (npcData.isSideQuest)
        {
            QuestSystem.instance.AddNewQuest(
                QuestSystem.instance.sideQuests[npcData.questIndex],
                false, false, true
            );
        }
        else if (npcData.isMainQuest)
        {
            QuestSystem.instance.AddNewQuest(
                QuestSystem.instance.quests[npcData.questIndex],
                true, false, false
            );
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
                    quest.questObjectAnnoun.SetActive(false);
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
                    quest.questObjectAnnoun.SetActive(false);
                    QuestSystem.instance.MarkQuestDone(-1, npcData.questIndex, false, false);
                    Debug.Log($"✅ Main Quest {npcData.questIndex} selesai (tanpa subquest).");
                }

                // 🟢 Tambahan → cek quest berikutnya dan set target
                if (quest.isDone)
                {
                    int nextIndex = npcData.questIndex + 1;
                    if (nextIndex < QuestSystem.instance.quests.Count)
                    {
                        var nextQuest = QuestSystem.instance.quests[nextIndex];
                        if (!nextQuest.isDone)
                        {
                            Debug.Log($"➡️ Lanjut ke Main Quest berikutnya: {nextQuest.text}");

                            // Tambahkan quest baru
                            // QuestSystem.instance.AddNewQuest(nextQuest, true, false, false);

                            // 🟢 Set target quest berikutnya
                            if (nextQuest.targetTransform != null)
                            {
                                QuestSystem.instance.questPathManager.SetQuestTarget(nextQuest.targetTransform);
                                Debug.Log($"🎯 Target diatur ke {nextQuest.targetTransform.name}");
                            }
                            else
                            {
                                Debug.LogWarning($"⚠️ Quest '{nextQuest.text}' belum punya targetTransform.");
                            }
                        }
                    }

                    if (npcData.questIndex == 0)
                    {
                        achieveManager.rewardButtons[0].interactable = true;
                        achieveAnnounAudio.Play();
                        LeanTween.moveY(achieveAnnoun.rectTransform, 280, .7f).setOnComplete(() =>
                        {            // Tunggu 5 detik sebelum melanjutkan
                            LeanTween.delayedCall(5f, () =>
                            {
                                LeanTween.moveY(achieveAnnoun.rectTransform, 430, .7f);
                            });
                        });

                        //LeanTween.moveLocalX(gerbangSekolah, 15f, 7f);

                        for (int i = 0; i < imageAnnoun.Length; i++)
                        {
                            imageAnnoun[i].SetActive(true);
                        }
                    }

                    if(npcData.questIndex == 2)
                    {
                        achieveManager.rewardButtons[2].interactable = true;
                        for (int i = 0; i < imageAnnoun.Length; i++)
                        {
                            imageAnnoun[i].SetActive(true);
                        }
                    }
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

                        // 🟢 Tambahan → lanjut ke side quest berikutnya
                        int nextIndex = npcData.questIndex + 1;
                        if (nextIndex < QuestSystem.instance.sideQuests.Count)
                        {
                            var nextSide = QuestSystem.instance.sideQuests[nextIndex];
                            //QuestSystem.instance.AddNewQuest(nextSide, false, false, true);

                            if (nextSide.targetTransform != null)
                            {
                                QuestSystem.instance.questPathManager.SetQuestTarget(nextSide.targetTransform);
                                Debug.Log($"🎯 Target side quest diatur ke {nextSide.targetTransform.name}");
                            }
                        }
                    }

                    //questObjectSide.SetActive(false);
                    //contohSideQuest.SetActive(false);
                }
                else
                {
                    // Tidak punya subquest → langsung done
                    QuestSystem.instance.MarkQuestDone(-1, npcData.questIndex, false, true);
                    Debug.Log($"✅ Side Quest {npcData.questIndex} selesai!");
                }
            }

            QuestSystem.instance.CheckAutoCompleteQuests();
            QuestSystem.instance.UpdateQuestDisplay();
        }
        else
        {
            Debug.Log($"💬 NPC {npcData.name} hanya ngobrol, tidak ada quest.");
        }

        chatPanel.SetActive(false);
        floatingButton.SetActive(false);
    }
}
