using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the desk surface object.
/// Player presses E on desk → checks inventory for all 4 pieces → opens assembly canvas.
/// Set Layer to Interactable.
/// </summary>
public class PosterAssemblyPuzzle : MonoBehaviour, IInteractable
{
    [Header("Piece Names — must match PickupExamineItem itemName exactly")]
    private string[] pieceNames = {
        "PosterPiece_TopLeft",
        "PosterPiece_TopRight",
        "PosterPiece_BottomLeft",
        "PosterPiece_BottomRight"
    };

    [Header("References")]
    [SerializeField] private GameObject posterAssemblyCanvas;

    [Header("Prompt")]
    [SerializeField] private string interactPrompt = "Assemble Poster";

    public void Interact(InteractionSystem interactor)
    {
        foreach (string piece in pieceNames)
        {
            if (!InventoryManager.Instance.HasItem(piece))
            {
                Debug.Log($"Missing: {piece}");
                return;
            }
        }

        foreach (string piece in pieceNames)
            InventoryManager.Instance.RemoveItem(piece);

        OpenAssemblyCanvas();
    }

    private void OpenAssemblyCanvas()
    {
        if (posterAssemblyCanvas != null)
        {
            posterAssemblyCanvas.SetActive(true);

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Lock player
            InteractionSystem interactor = FindFirstObjectByType<InteractionSystem>();
            if (interactor != null) interactor.EnterInteractionMode();

            // Also directly find and lock the camera
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                pm.SetMovementLocked(true);
                pm.SetLookLocked(true);
            }
        }
    }

    public string GetInteractionPrompt()
    {
        // Show different prompts depending on how many pieces collected
        int count = 0;
        foreach (string piece in pieceNames)
            if (InventoryManager.Instance.HasItem(piece)) count++;

        if (count == 4)
            return $"{interactPrompt}";
        else
            return $"Find poster pieces ({count}/4)";
    }
}
