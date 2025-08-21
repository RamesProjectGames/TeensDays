using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Reward 
{
    public string rewardName;   // Nama reward
    public Sprite icon;         // Icon reward di UI
    public int amount;          // Jumlah reward
    public bool isSpecial;      // True kalau hadiah spesial (gift box)
}
