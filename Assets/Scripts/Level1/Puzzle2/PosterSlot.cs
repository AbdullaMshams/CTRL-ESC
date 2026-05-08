using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to each of the 4 empty slots on the RIGHT panel (assembly grid).
/// Each slot only accepts one specific piece ID.
/// Notifies PosterAssemblyManager when correctly filled.
/// </summary>
public class PosterSlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Identity")]
    [Tooltip("Must match DraggablePiece pieceID exactly — e.g. TopLeft")]
    public string acceptedPieceID;

    [Header("References")]
    [SerializeField] private PosterAssemblyManager assemblyManager;

    [Header("Visual Feedback")]
    [SerializeField] private Image slotImage;           // The slot background image
    [SerializeField] private Color emptyColor   = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color correctColor = new Color(0.3f, 1f, 0.3f, 0.5f);
    [SerializeField] private Color wrongColor   = new Color(1f, 0.3f, 0.3f, 0.5f);

    private bool isFilled = false;

    private void Start()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();

        if (slotImage != null)
            slotImage.color = emptyColor;

        // Auto-find manager if not assigned
        if (assemblyManager == null)
            assemblyManager = FindFirstObjectByType<PosterAssemblyManager>();
    }

    // Called by Unity when something is dropped onto this slot
    public void OnDrop(PointerEventData eventData)
    {
        // Don't accept more pieces if already filled
        if (isFilled) return;

        // Get the DraggablePiece from the dropped object
        DraggablePiece droppedPiece = eventData.pointerDrag?.GetComponent<DraggablePiece>();

        if (droppedPiece == null) return;

        // Check if the dropped piece matches what this slot accepts
        if (droppedPiece.pieceID == acceptedPieceID)
        {
            // ✅ Correct piece!
            isFilled = true;

            // Snap piece into this slot
            droppedPiece.PlaceInSlot(transform);

            // Visual feedback — green
            if (slotImage != null)
                slotImage.color = correctColor;

            // Notify the manager
            if (assemblyManager != null)
                assemblyManager.OnPiecePlaced();

            Debug.Log($"Correct! {droppedPiece.pieceID} placed in {acceptedPieceID} slot.");
        }
        else
        {
            // ❌ Wrong piece — flash red then snap back
            Debug.Log($"Wrong piece! Expected {acceptedPieceID}, got {droppedPiece.pieceID}");

            // Flash wrong color briefly
            if (slotImage != null)
                StartCoroutine(FlashWrongColor());

            // Piece will snap back automatically via DraggablePiece.OnEndDrag
        }
    }

    private System.Collections.IEnumerator FlashWrongColor()
    {
        if (slotImage != null)
        {
            slotImage.color = wrongColor;
            yield return new WaitForSeconds(0.4f);
            slotImage.color = emptyColor;
        }
    }
}
