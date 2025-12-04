using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCDialogData
{
    public string id;                  // ID unik NPC
    public string name;                // Nama NPC
    public List<DialogLine> dialog;        // Percakapan default

    // --- Quest Info ---
    public bool givesQuest;            // Apakah NPC ini memberikan quest
    public bool isMainQuest;           // True jika quest utama (tanpa subquest)
    public bool isSubQuest;            // True jika subquest dari quest utama
    public bool isSideQuest;           // True jika side quest
    public int parentIndex;            // Index quest utama (hanya untuk subquest)
    public int questIndex;             // Index quest / subquest / sidequest
    public int subQuestIndex;
}

[System.Serializable]
public class NPCDialogList
{
    public NPCDialogData[] npcs;
}

[System.Serializable]
public class DialogLine
{
    public string speaker;   // "IBU" atau "BOBON"
    public bool isPlayer;    // true = player, false = NPC
    public string text;      // isi dialog
}
