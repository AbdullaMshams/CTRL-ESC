using UnityEngine;
using TMPro;
using UnityEngine.UI;


/// <summary>
/// Handles player interaction with objects in the world.
/// Uses raycast from the camera to detect interactable objects.
/// Objects must have the "Interactable" tag and implement IInteractable interface.
/// Attach this script to the Player GameObject.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    private ExamineSystem examineSystem;
    private FocusSystem focusSystem;
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;

    // [Header("Crosshair Settings")]
    // [SerializeField] private Texture2D defaultCrosshair;
    // [SerializeField] private Texture2D interactCrosshair;
    // [SerializeField] private int crosshairSize = 32;

    [Header("Crosshair")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color interactCrosshairColor = Color.yellow;

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

        if (crosshairImage != null)
            crosshairImage.color = defaultCrosshairColor;

        // Hide prompt on start
        if (interactionPromptUI != null)
            interactionPromptUI.SetActive(false);

        focusSystem = GetComponent<FocusSystem>();
        examineSystem = GetComponent<ExamineSystem>();
    }

    private void Update()
    {
        HandleRaycast();
        HandleInteractionInput();
    }

    private void UpdateCrosshair(bool isLooking)
    {
        if (crosshairImage != null)
            crosshairImage.color = isLooking ? interactCrosshairColor : defaultCrosshairColor;
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
                currentInteractable = interactable;
                isLookingAtInteractable = true;
                ShowPrompt(interactable.GetInteractionPrompt());
                UpdateCrosshair(true);
                return;
            }
        }

        currentInteractable = null;
        isLookingAtInteractable = false;
        HidePrompt();
        UpdateCrosshair(false);
    }

    /// <summary>
    /// Checks for E key press and triggers interaction.
    /// </summary>
private void HandleInteractionInput()
{
    if (examineSystem != null && examineSystem.IsExamining()) return;
    if (focusSystem != null && focusSystem.IsFocusing()) return;

    if (Input.GetKeyDown(KeyCode.E) && isLookingAtInteractable 
        && currentInteractable != null)
    {
        currentInteractable.Interact(this);
    }
}
private void TryExamineInventoryItem()
{
    if (InventoryManager.Instance == null) return;

    GameObject selectedObj = InventoryManager.Instance.GetSelectedItemObject();
    if (selectedObj == null) return;

    // Bring item out to examine
    selectedObj.SetActive(true);
    selectedObj.transform.position = playerCamera.transform.position 
        + playerCamera.transform.forward * 0.5f;

    PickupExamineItem pickupItem = selectedObj.GetComponent<PickupExamineItem>();
    examineSystem.StartExamining(selectedObj, this, pickupItem);

    // Remove from inventory temporarily while examining
    InventoryManager.Instance.RemoveSelectedItemTemporarily();
}

    /// <summary>
    /// Draws the crosshair on screen.
    /// Changes appearance when looking at interactable object.
    /// </summary>
 

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