using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PosterAssemblyManager : MonoBehaviour
{
    [Header("Piece Names")]
    private string[] pieceNames = {
        "PosterPiece_TopLeft",
        "PosterPiece_TopRight",
        "PosterPiece_BottomLeft",
        "PosterPiece_BottomRight"
    };

    [Header("Panels")]
    [SerializeField] private GameObject leftPanel;
    [SerializeField] private GameObject rightPanel;
    [SerializeField] private GameObject completedPosterPanel;

    [Header("Completed Poster UI")]
    [SerializeField] private Image completedPosterImage;
    [SerializeField] private TextMeshProUGUI urlText;
    [SerializeField] private string portalURL = "portal.nexusdynamics.internal";

    [Header("Close Button")]
    [SerializeField] private GameObject closeButton;

    [Header("Frame Reveal")]
    [SerializeField] private GameObject posterRevealQuad; // The quad inside the frame

    private int placedCount = 0;
    private const int TOTAL = 4;
    private bool puzzleSolved = false;

    private void Start()
    {
        if (completedPosterPanel != null)
            completedPosterPanel.SetActive(false);
        if (urlText != null)
            urlText.text = "";
        if (closeButton != null)
            closeButton.SetActive(false);

        // Make sure poster is hidden at start
        if (posterRevealQuad != null)
            posterRevealQuad.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!puzzleSolved)
                Debug.Log("Player closed puzzle — pieces still in inventory.");
            CloseCanvas();
        }
    }

    public void OnPiecePlaced()
    {
        placedCount++;
        Debug.Log($"Pieces placed: {placedCount}/{TOTAL}");

        if (placedCount >= TOTAL && !puzzleSolved)
        {
            puzzleSolved = true;

            // Remove the 4 pieces from inventory
            foreach (string piece in pieceNames)
                InventoryManager.Instance.RemoveItem(piece.Trim());

            RevealCompletedPoster();
        }
    }

    private void RevealCompletedPoster()
    {
        StartCoroutine(RevealWithDelay());
    }

    private System.Collections.IEnumerator RevealWithDelay()
    {
        yield return new WaitForSeconds(0.8f);

       
        if (leftPanel != null) leftPanel.SetActive(false);
        if (rightPanel != null) rightPanel.SetActive(false);

        
        if (completedPosterPanel != null)
            completedPosterPanel.SetActive(true);

        
        if (urlText != null)
            urlText.text = portalURL;

        
        if (closeButton != null)
            closeButton.SetActive(true);

        
        if (posterRevealQuad != null)
        {
            posterRevealQuad.SetActive(true);
            Debug.Log("Poster revealed in frame!");
        }

        Debug.Log($"Nexus Dynamics portal revealed: {portalURL}");
    }

    public void CloseCanvas()
    {
        if (!puzzleSolved)
        {
            placedCount = 0;
            ResetSlots();
            ResetPieces();
        }

        
        if (!puzzleSolved)
        {
            if (leftPanel != null) leftPanel.SetActive(true);
            if (rightPanel != null) rightPanel.SetActive(true);
        }

        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InteractionSystem interactor = FindFirstObjectByType<InteractionSystem>();
        if (interactor != null) interactor.ExitInteractionMode();

        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
        {
            pm.SetMovementLocked(false);
            pm.SetLookLocked(false);
        }

        Debug.Log("Assembly canvas closed.");
    }

    private void ResetSlots()
    {
        PosterSlot[] slots = GetComponentsInChildren<PosterSlot>(true);
        foreach (PosterSlot slot in slots)
            slot.ResetSlot();
    }

    private void ResetPieces()
    {
        DraggablePiece[] pieces = GetComponentsInChildren<DraggablePiece>(true);
        foreach (DraggablePiece piece in pieces)
            piece.ResetToOriginal();
    }
}