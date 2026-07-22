using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentManager : MonoBehaviour
{
    [Header("Assignment Progress")]
    [SerializeField, Range(0f, 1f)] protected float progressCompletion;
    [SerializeField] protected bool isStarted;
    [SerializeField] protected bool isCompleted;

    public float ProgressCompletion => progressCompletion;
    public bool IsStarted => isStarted;
    public bool IsCompleted => isCompleted;

    public virtual void ActivateQuest()
    {
        // Derived managers decide when quest is considered started.
    }

    public virtual void DeactivateQuest()
    {
        QuestPathManager.Instance.SetQuestTarget(null);
    }

    protected void MarkStarted()
    {
        isStarted = true;
    }

    protected void SetProgress(float progress, bool autoComplete = false)
    {
        progressCompletion = Mathf.Clamp01(progress);
        if (progressCompletion > 0f)
        {
            isStarted = true;
        }

        if (autoComplete && progressCompletion >= 1f)
        {
            CompleteProgress();
        }
    }

    protected void CompleteProgress()
    {
        isStarted = true;
        isCompleted = true;
        progressCompletion = 1f;
    }

    protected void ResetProgress()
    {
        progressCompletion = 0f;
        isStarted = false;
        isCompleted = false;
    }

    protected void TrackProgressFromSubQuests(string parentQuestName, bool isSideQuest = true, int startSubQuestIndex = 0)
    {
        if (QuestSystem.instance == null || string.IsNullOrEmpty(parentQuestName))
            return;

        Quest quest = QuestSystem.instance.GetQuest(parentQuestName, isSideQuest);
        if (quest == null)
            return;

        int safeStartIndex = Mathf.Clamp(startSubQuestIndex, 0, Mathf.Max(0, quest.subQuests.Count - 1));
        int total = 0;
        int done = 0;

        for (int i = safeStartIndex; i < quest.subQuests.Count; i++)
        {
            total++;
            if (quest.subQuests[i].isDone)
            {
                done++;
            }
        }

        if (done > 0)
        {
            MarkStarted();
        }

        if (total <= 0)
        {
            if (quest.isDone)
            {
                CompleteProgress();
            }
            else
            {
                SetProgress(0f);
            }
            return;
        }

        SetProgress((float)done / total);

        if (quest.isDone || done >= total)
        {
            CompleteProgress();
        }
    }

    protected void LoadProgressFromQuestState(string parentQuestName, bool isSideQuest = true, int startSubQuestIndex = 0)
    {
        ResetProgress();

        if (QuestSystem.instance == null || string.IsNullOrEmpty(parentQuestName))
            return;

        Quest quest = QuestSystem.instance.GetQuest(parentQuestName, isSideQuest);
        if (quest == null)
            return;

        if (quest.isDone)
        {
            CompleteProgress();
            return;
        }

        int safeStartIndex = Mathf.Clamp(startSubQuestIndex, 0, Mathf.Max(0, quest.subQuests.Count - 1));
        int total = 0;
        int done = 0;

        for (int i = safeStartIndex; i < quest.subQuests.Count; i++)
        {
            total++;
            if (quest.subQuests[i].isDone)
            {
                done++;
            }
        }

        if (total > 0)
        {
            SetProgress((float)done / total);
            if (done >= total)
            {
                CompleteProgress();
                return;
            }
        }

        int questIndex = QuestSystem.instance.GetQuestIndex(parentQuestName, isSideQuest);
        if (questIndex >= 0)
        {
            bool isOngoing = isSideQuest
                ? QuestSystem.instance.GetCurrentSideQuestIndex() == questIndex
                : QuestSystem.instance.GetCurrentQuestIndex() == questIndex;

            if (isOngoing || done > 0)
            {
                MarkStarted();
            }
        }
    }
}
