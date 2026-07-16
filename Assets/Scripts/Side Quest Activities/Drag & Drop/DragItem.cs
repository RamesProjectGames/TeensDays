using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Data")]
    public int CorrectOrder;

    [HideInInspector]
    public DragSlot CurrentSlot;

    [Header("UI")]
    [SerializeField] private TMP_Text numberText;

    [Header("Settings")]
    [SerializeField] private float swapDuration = 0.25f;
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private float dragAlpha = 0.8f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rect;
    public RectTransform RectTransform { get; private set; }

    private Vector3 startPosition;
    private Vector3 startScale;


    public bool IsDragging { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        RectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();

        startScale = transform.localScale;
    }

    public void SetNumber(int number)
    {
        CorrectOrder = number;
        numberText.text = number.ToString();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (LeanTween.isTweening(gameObject))
            return;

        IsDragging = true;

        startPosition = rect.position;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = dragAlpha;

        transform.localScale = startScale * dragScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint);

        rect.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.localScale = startScale;

        DragSlot nearest = DragAndDropManager.Instance.GetNearestSlot(rect.position);

        if (nearest.CurrentItem != null)
        {
            Swap(nearest.CurrentItem);
        }
        else
        {
            MoveBack();
        }
    }

    void MoveBack()
    {
        LeanTween.move(gameObject, CurrentSlot.transform.position, 0.15f)
            .setEaseOutQuad();
    }

    void Swap(DragItem other)
    {
        if (other == this)
        {
            MoveBack();
            return;
        }

        DragSlot mySlot = CurrentSlot;
        DragSlot otherSlot = other.CurrentSlot;

        // Update slot ownership
        mySlot.CurrentItem = other;
        otherSlot.CurrentItem = this;

        CurrentSlot = otherSlot;
        other.CurrentSlot = mySlot;

        LeanTween.move(gameObject, otherSlot.transform.position, swapDuration)
            .setEaseOutQuad();

        LeanTween.move(other.gameObject, mySlot.transform.position, swapDuration)
            .setEaseOutQuad()
            .setOnComplete(() =>
            {
                DragAndDropManager.Instance.CheckSolved();
            });
    }
    private DragItem GetTarget(PointerEventData eventData)
    {
        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            DragItem item = result.gameObject.GetComponentInParent<DragItem>();

            if (item != null && item != this)
                return item;
        }

        return null;
    }
}