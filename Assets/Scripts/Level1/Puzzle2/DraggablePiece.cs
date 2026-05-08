using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to each of the 4 piece images on the LEFT panel.
/// Handles drag, follow mouse, snap back if not dropped on valid slot.
/// </summary>
public class DraggablePiece : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Piece Identity")]
    public string pieceID; // e.g. "TopLeft", "TopRight", "BottomLeft", "BottomRight"

    // Private state
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform  = GetComponent<RectTransform>();
        canvasGroup    = GetComponent<CanvasGroup>();
        canvas         = GetComponentInParent<Canvas>();

        // Add CanvasGroup if missing — needed to pass raycasts to slots below
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // Called once when drag starts
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Save original position and parent so we can snap back if needed
        originalPosition = rectTransform.anchoredPosition;
        originalParent   = transform.parent;

        // Move to top of canvas so it renders above everything
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Disable raycasts on this image so the slot below can receive the drop
        canvasGroup.blocksRaycasts = false;

        Debug.Log($"Started dragging: {pieceID}");
    }

    // Called every frame while dragging
    public void OnDrag(PointerEventData eventData)
    {
        // Move piece with mouse — divide by canvas scale for correct position
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // Called once when drag ends (released)
    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable raycasts
        canvasGroup.blocksRaycasts = true;

        // If not dropped on a valid slot (PosterSlot handles snapping),
        // snap back to original position and parent
        if (transform.parent == canvas.transform)
        {
            SnapBack();
        }

        Debug.Log($"Ended dragging: {pieceID}");
    }

    /// <summary>
    /// Called by PosterSlot when piece is correctly placed.
    /// Keeps piece in the slot — do not snap back.
    /// </summary>
    public void PlaceInSlot(Transform slotTransform)
    {
        transform.SetParent(slotTransform);
        rectTransform.anchoredPosition = Vector2.zero;
        canvasGroup.blocksRaycasts     = true;
        Debug.Log($"{pieceID} placed correctly in slot!");
    }

    /// <summary>
    /// Snaps piece back to its original position in the left panel.
    /// Called when dropped on wrong slot or empty space.
    /// </summary>
    public void SnapBack()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
        Debug.Log($"{pieceID} snapped back to original position.");
    }
}
