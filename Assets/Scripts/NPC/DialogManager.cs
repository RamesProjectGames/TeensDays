using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    [SerializeField] private NPCDialogList dialogList;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Load JSON dari Resources
        TextAsset jsonFile = Resources.Load<TextAsset>("DialogNpc");
        if (jsonFile != null)
        {
            dialogList = JsonUtility.FromJson<NPCDialogList>(jsonFile.text);
        }
        else
        {
            Debug.LogError("Dialog JSON not found!");
        }
    }

    public NPCDialogData GetDialogByID(string npcId)
    {
        foreach (var npc in dialogList.npcs)
        {
            if (npc.id == npcId)
                return npc;
        }
        return null;
    }
}
