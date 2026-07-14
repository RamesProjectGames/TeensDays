using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentManager : MonoBehaviour
{
    public virtual void ActivateQuest()
    {
        QuestPathManager.Instance.SetQuestTarget(null);
    }
    public virtual void DeactivateQuest()
    {
        QuestPathManager.Instance.SetQuestTarget(null);
    }
}
