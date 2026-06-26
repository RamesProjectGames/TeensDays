using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaintManager : MonoBehaviour
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
    public Color brushColor = Color.red;
    [Range(1,100)]
    public int brushRadius = 20;

    [Header("Completion")]
    [Range(0f,1f)]
    public float completionRequirement = 0.95f;

    [Tooltip("White pixels = required paint area")]
    [SerializeField] private Image targetImage;
    private Texture2D targetTexture;

    public UnityEvent<float> onProgressChanged;
    public UnityEvent onCompleted;

    private RawImage rawImage;
    private Texture2D paintTexture;

    private bool completed;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();

        targetTexture = targetImage.sprite.texture;

        paintTexture = new Texture2D(
            targetTexture.width,
            targetTexture.height,
            TextureFormat.RGBA32,
            false
        );
        paintTexture.filterMode = FilterMode.Point;

        ClearTexture();

        rawImage.texture = paintTexture;
    }
    void Start()
    {
         if(NPCRelated.TryGetComponent<InteractableNPC>(out var interactable))
        {
            this.interactable = interactable;
        }
        NPCRelated.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Paint();

            CheckCompletion();
        }
    }
    public void ActivateQuest()
    {        
        NPCRelated.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartPaint);
        }
    }
    public void StartPaint()
    {
        NPCRelated.SetActive(false);
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
        }
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,0,true);
        QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,"Paint Progress = 0%");
    }
    public void FinishPaint()
    {
        NPCRelated.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.AddListener(() =>
            {
                QuestSystem.instance.MarkQuestDone(4, 1, true, true);
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
                }
                ResetQuest();
            });
        }        
        QuestSystem.instance.MarkQuestDone(4, 0, true, true);
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,1,true);
        
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
    void Paint()
    {
        Vector2 localPoint;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImage.rectTransform,
            Input.mousePosition,
            null,
            out localPoint))
            return;

        Rect rect = rawImage.rectTransform.rect;

        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        int x = Mathf.RoundToInt(normalizedX * paintTexture.width);
        int y = Mathf.RoundToInt(normalizedY * paintTexture.height);

        DrawCircle(x, y);
    }

    void DrawCircle(int cx, int cy)
    {
        int rSquared = brushRadius * brushRadius;

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

                paintTexture.SetPixel(px, py, brushColor);
            }
        }

        paintTexture.Apply();
    }

    public void ClearTexture()
    {
        Color clear = new Color(0,0,0,0);

        Color[] pixels = new Color[targetTexture.width * targetTexture.height];

        for(int i=0;i<pixels.Length;i++)
            pixels[i] = clear;

        paintTexture.SetPixels(pixels);
        paintTexture.Apply();

        completed = false;
    }

    void CheckCompletion()
    {
        if (targetTexture == null)
            return;

        int required = 0;
        int painted = 0;

        for (int x = 0; x < targetTexture.width; x++)
        {
            for (int y = 0; y < targetTexture.height; y++)
            {
                Color target = targetTexture.GetPixel(x, y);

                if (target.a < 0.5f)
                    continue;

                required++;

                Color player = paintTexture.GetPixel(x, y);

                if (player.a > 0.1f)
                    painted++;
            }
        }

        float progress = required == 0 ? 0 : (float)painted / required;

        
        QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,$"Paint Progress = {progress * 100}%");

        onProgressChanged?.Invoke(progress);

        if (!completed && progress >= completionRequirement)
        {
            completed = true;
            onCompleted?.Invoke();
        }
    }

    public float GetCompletion()
    {
        if (targetTexture == null)
            return 0;

        int required = 0;
        int painted = 0;

        for (int x = 0; x < targetTexture.width; x++)
        {
            for (int y = 0; y < targetTexture.height; y++)
            {
                if (targetTexture.GetPixel(x, y).a < 0.5f)
                    continue;

                required++;

                if (paintTexture.GetPixel(x,y).a > 0.1f)
                    painted++;
            }
        }

        return required == 0 ? 0 : (float)painted / required;
    }
}
