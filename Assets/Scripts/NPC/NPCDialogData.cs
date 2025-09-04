using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCDialogData
{
    public string id;
    public string name;
    public string[] dialog;
}

[System.Serializable]
public class NPCDialogList
{
    public NPCDialogData[] npcs;
}
