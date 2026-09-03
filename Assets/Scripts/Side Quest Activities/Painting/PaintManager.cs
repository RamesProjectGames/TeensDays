using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaintManager : AssignmentManager
{
    [Header("Quest Related")]
    
    public string questName;
    public GameObject NPCRelated;
    public string inCompleteDialogue;
    public string completedDialogue;
    public int rewardAmount;
    public int repeatableRewradAmount;    
    InteractableNPC interactable;
    [Header("Painting")]
    public GameObject paintingUI;
    [Range(1,500)]
    public int brushRadius = 20;

    [Header("Completion")]
    [Range(0f,1f)]
    public float completionRequirement = 0.95f;

    [Tooltip("Mask image. Visible pixels define the area that can be painted.")]
    [SerializeField] private Image targetImage;
    [Tooltip("Unfinished image displayed above the finished image.")]
    [SerializeField] private RawImage rawImage;
    private Texture2D targetTexture;
    private Texture2D unfinishedTexture;

    public UnityEvent<float> onProgressChanged;
    public UnityEvent onCompleted;

    private Texture2D paintTexture;
    private Color[] targetPixels;
    private Color[] paintPixels;
    private bool completed, startPaint;
    private bool completionCheckQueued;

    void Awake()
    {
        // rawImage = GetComponent<RawImage>();

        targetTexture = targetImage.sprite.texture;
        targetPixels = targetTexture.GetPixels();

        unfinishedTexture = rawImage.texture as Texture2D;
        if (unfinishedTexture == null)
        {
            Debug.LogError("PaintManager requires Raw Image to use a readable Texture2D.", this);
            enabled = false;
            return;
        }

        paintTexture = new Texture2D(
            unfinishedTexture.width,
            unfinishedTexture.height,
            TextureFormat.RGBA32,
            false
        );
        paintTexture.filterMode = FilterMode.Point;

        ResetPaintTexture();

        rawImage.texture = paintTexture;
    }
    void Start()
    {
        if(NPCRelated.TryGetComponent<InteractableNPC>(out var interactable))
        {
            this.interactable = interactable;
        }
        NPCRelated.SetActive(false);
        paintingUI.SetActive(false);
    }

    void Update()
    {
        if(!startPaint) return;
        if (Input.GetMouseButton(0))
        {
            if (Paint())
            {
                completionCheckQueued = true;
            }

            if (completionCheckQueued && Time.frameCount % 3 == 0)
            {
                CheckCompletion();
                completionCheckQueued = false;
            }
        }
    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true);
        NPCRelated.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.onTalkEnded.AddListener(StartPaint);
        }
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        NPCRelated.SetActive(false);
        paintingUI.SetActive(false);
    }
    [ContextMenu("Start Paint")]
    public void StartPaint()
    {
        MarkStarted();
        SetProgress(0f);
        NPCRelated.SetActive(false);
        paintingUI.SetActive(true);
        startPaint = true;
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
        }
        // QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,0,true);
        QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,"Paint Progress = 0%");
    }
    public void FinishPaint()
    {
        NPCRelated.SetActive(true);
        paintingUI.SetActive(false);
        startPaint= false;
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.AddListener(() =>
            {
                QuestSystem.instance.MarkQuestDone(4, 1, true, true);
                TrackProgressFromSubQuests(questName, true);
                var questRelated = QuestSystem.instance.GetQuest(questName, true);
                if (questRelated != null)
                {
                    if (!questRelated.isDone)
                    {
                        GameManager.Instance.playerData.currMoney += rewardAmount;
                    }
                    else
                    {
                        GameManager.Instance.playerData.currMoney += repeatableRewradAmount;
                    }

                    if (questRelated.isDone)
                    {
                        CompleteProgress();
                    }
                }
                ResetQuest();
            });
        }        
        QuestSystem.instance.MarkQuestDone(4, 0, true, true);
        TrackProgressFromSubQuests(questName, true);
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,QuestSystem.instance.GetSubQuestIndex(questName,completedDialogue,true),true);
        
    }
    public void ResetQuest()
    {
        NPCRelated.SetActive(false);
        if(interactable != null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartPaint);
        }
    }    
    bool Paint()
    {
        Vector2 localPoint;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            Input.mousePosition,
            null,
            out localPoint))
            return false;

        Rect rect = rawImage.rectTransform.rect;

        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        int x = Mathf.RoundToInt(normalizedX * paintTexture.width);
        int y = Mathf.RoundToInt(normalizedY * paintTexture.height);

        return DrawCircle(x, y);
    }

    bool DrawCircle(int cx, int cy)
    {
        int rSquared = brushRadius * brushRadius;
        bool changed = false;

        for (int x = -brushRadius; x <= brushRadius; x++)
        {
            for (int y = -brushRadius; y <= brushRadius; y++)
            {
                if (x * x + y * y > rSquared)
                    continue;

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || py < 0 || px >= paintTexture.width || py >= paintTexture.height)
                    continue;

                int index = py * paintTexture.width + px;

                if (targetPixels[index].a < 0.5f)
                    continue;

                if (paintPixels[index].a <= 0.1f)
                    continue;

                paintPixels[index] = Color.clear;
                changed = true;
            }
        }

        if (!changed)
            return false;

        paintTexture.SetPixels(paintPixels);
        paintTexture.Apply(false);
        return true;
    }

    public void ClearTexture()
    {
        ResetPaintTexture();

        completionCheckQueued = false;
        completed = false;
    }

    private void ResetPaintTexture()
    {
        paintPixels = unfinishedTexture.GetPixels();

        paintTexture.SetPixels(paintPixels);
        paintTexture.Apply(false);
    }

    void CheckCompletion()
    {
        if (targetTexture == null || targetPixels == null || paintPixels == null)
            return;

        int required = 0;
        int painted = 0;

        for (int i = 0; i < targetPixels.Length; i++)
        {
            if (targetPixels[i].a < 0.5f)
                continue;

            required++;

            if (paintPixels[i].a <= 0.1f)
                painted++;
        }

        float progress = required == 0 ? 0 : (float)painted / required;
        SetProgress(progress);

        QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,$"Paint Progress = {progress * 100}%");

        onProgressChanged?.Invoke(progress);

        if (!completed && progress >= completionRequirement)
        {
            completed = true;
            QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,$"Done painting! talk to pak bagus");
            CompleteProgress();
            onCompleted?.Invoke();
        }
    }

    public float GetCompletion()
    {
        if (targetTexture == null || targetPixels == null || paintPixels == null)
            return 0;

        int required = 0;
        int painted = 0;

        for (int i = 0; i < targetPixels.Length; i++)
        {
            if (targetPixels[i].a < 0.5f)
                continue;

            required++;

            if (paintPixels[i].a <= 0.1f)
                painted++;
        }

        return required == 0 ? 0 : (float)painted / required;
    }
}
