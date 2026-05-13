using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PosterAssemblyPuzzle : MonoBehaviour, IInteractable
{
    [Header("Piece Names")]
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
        // Null check
        if (InventoryManager.Instance == null) return;

        foreach (string piece in pieceNames)
        {
            if (!InventoryManager.Instance.HasItem(piece.Trim()))
            {
                Debug.Log($"Missing: {piece}");
                return;
            }
        }

        Debug.Log("All pieces collected! Opening assembly canvas.");
        OpenAssemblyCanvas();
    }

    private void OpenAssemblyCanvas()
    {
        if (posterAssemblyCanvas != null)
        {
            posterAssemblyCanvas.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            InteractionSystem interactor = FindFirstObjectByType<InteractionSystem>();
            if (interactor != null) interactor.EnterInteractionMode();

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
        // Null check — prevents crash on scene start
        if (InventoryManager.Instance == null)
            return interactPrompt;

        int count = 0;
        foreach (string piece in pieceNames)
            if (InventoryManager.Instance.HasItem(piece.Trim())) count++;

        if (count == 4)
            return interactPrompt;
        else
            return $"Find poster pieces ({count}/4)";
    }
}