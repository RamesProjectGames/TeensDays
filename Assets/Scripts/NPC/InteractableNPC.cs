using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableNPC : MonoBehaviour
{
    public string npcId;
    public UnityEvent OnTalkStart;
    public UnityEvent onTalkEnded;
}
