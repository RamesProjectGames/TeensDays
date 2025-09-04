using System.Collections;
using System.Collections.Generic;
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
            }
            else
            {
                Debug.LogWarning("NPC dialog not found for id: " + currentNPC.npcId);
            }
        }
    }

    public void QuestDone()
    {
        questSystem.MarkQuestDone(0, 0);
        chatPanel.SetActive(false);
        floatingButton.SetActive(false);
        //int valQuest = QuestData.x;
        //int valSubQuest = QuestData.y;
    }
}
