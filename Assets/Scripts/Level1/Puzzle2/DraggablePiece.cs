using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePiece : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Piece Identity")]
    public string pieceID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;

    // Store the HOME position — set once and never changes
    private Vector2 homePosition;
    private Transform homeParent;
    private bool homeSet = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        // Save home position ONCE on first enable
        if (!homeSet)
        {
            homePosition = rectTransform.anchoredPosition;
            homeParent = transform.parent;
            homeSet = true;
        }

        // Always reset visuals when panel opens
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == canvas.transform)
            SnapBack();
    }

    public void PlaceInSlot(Transform slotTransform)
    {
        transform.SetParent(slotTransform);
        rectTransform.anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
    }

    public void SnapBack()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }

    // Called by PosterAssemblyManager when ESC is pressed
    public void ResetToOriginal()
    {
        transform.SetParent(homeParent);
        rectTransform.anchoredPosition = homePosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        gameObject.SetActive(true);
    }
}