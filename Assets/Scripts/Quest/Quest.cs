using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public TMP_Text questText;
    public string text;
    public bool isDone;

    public List<Quest> subQuests = new List<Quest>(); // Tambahkan ini

    public bool IsFullyCompleted()
    {
        // Quest utama selesai jika dirinya dan semua subquest selesai
        return isDone && subQuests.All(sq => sq.isDone);
    }
}
