using UnityEngine;
using TMPro;


/// <summary>
/// Handles player interaction with objects in the world.
/// Uses raycast from the camera to detect interactable objects.
/// Objects must have the "Interactable" tag and implement IInteractable interface.
/// Attach this script to the Player GameObject.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
private ExamineSystem examineSystem;
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Crosshair Settings")]
    [SerializeField] private Texture2D defaultCrosshair;
    [SerializeField] private Texture2D interactCrosshair;
    [SerializeField] private int crosshairSize = 32;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TextMeshProUGUI interactionPromptText;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    // Private variables
    private IInteractable currentInteractable;
    private bool isLookingAtInteractable = false;

    private void Start()
    {

        // Auto find references if not assigned
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        // Hide prompt on start
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
        
    examineSystem = GetComponent<ExamineSystem>();
    }

    private void Update()
    {
        HandleRaycast();
        HandleInteractionInput();
    }

    /// <summary>
    /// Shoots a raycast from the camera center.
    /// Detects objects that implement IInteractable.
    /// Changes crosshair and shows prompt when looking at interactable.
    /// </summary>
    private void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Looking at something interactable
                currentInteractable = interactable;
                isLookingAtInteractable = true;

                // Show interaction prompt
                ShowPrompt(interactable.GetInteractionPrompt());
                return;
            }
        }

        // Not looking at anything interactable
        currentInteractable = null;
        isLookingAtInteractable = false;
        HidePrompt();
    }

    /// <summary>
    /// Checks for E key press and triggers interaction.
    /// </summary>
  private void HandleInteractionInput()
{
    if (examineSystem != null && examineSystem.IsExamining()) return;

    if (Input.GetKeyDown(KeyCode.E) && isLookingAtInteractable && currentInteractable != null)
    {
        currentInteractable.Interact(this);
    }
}

    /// <summary>
    /// Draws the crosshair on screen.
    /// Changes appearance when looking at interactable object.
    /// </summary>
    private void OnGUI()
    {
        Texture2D crosshair = isLookingAtInteractable ? interactCrosshair : defaultCrosshair;

        if (crosshair != null)
        {
            float x = (Screen.width  - crosshairSize) / 2f;
            float y = (Screen.height - crosshairSize) / 2f;
            GUI.DrawTexture(new Rect(x, y, crosshairSize, crosshairSize), crosshair);
        }
        else
        {
            // Fallback dot crosshair if no texture assigned
            float x = (Screen.width  - 4) / 2f;
            float y = (Screen.height - 4) / 2f;
            Color color = isLookingAtInteractable ? Color.yellow : Color.white;
            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, 4, 4), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }

    // ── Prompt Helpers ────────────────────────────────────────────────────────

    private void ShowPrompt(string text)
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
            if (interactionPromptText != null)
                interactionPromptText.text = text;
        }
    }

    private void HidePrompt()
    {
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);
    }

    // ── Public Methods ────────────────────────────────────────────────────────

    /// <summary>
    /// Lock player movement and look when entering a puzzle interaction.
    /// Call this from puzzle scripts when they open their UI.
    /// </summary>
    public void EnterInteractionMode()
    {
        playerMovement.SetMovementLocked(true);
        playerMovement.SetLookLocked(true);
    }

    /// <summary>
    /// Unlock player movement and look when exiting a puzzle interaction.
    /// Call this from puzzle scripts when they close their UI.
    /// </summary>
    public void ExitInteractionMode()
    {
        playerMovement.SetMovementLocked(false);
        playerMovement.SetLookLocked(false);
    }
}