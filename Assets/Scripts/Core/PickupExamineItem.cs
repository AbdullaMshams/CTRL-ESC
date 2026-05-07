using UnityEngine;

/// <summary>
/// Combined pickup and examine script.
/// Press E to examine → Press E again to add to inventory → Press Q to drop back.
/// Attach to any item that can be picked up.
/// Set Layer to Interactable.
/// </summary>
public class PickupExamineItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private string examinePrompt = "Examine";

    [Header("Examine Override Settings")] // Add these lines
    public float minZoom = 0.5f;
    public float maxZoom = 2.5f;
    public float startZoom = 1.0f;
    public float targetSize = 0.15f;

    private ExamineSystem examineSystem;
    private bool isBeingExamined = false;

    private void Start()
    {
        examineSystem = FindFirstObjectByType<ExamineSystem>();
    }

    public void Interact(InteractionSystem interactor)
    {
        // Start examining
        if (!isBeingExamined)
        {
            isBeingExamined = true;
            examineSystem.StartExamining(gameObject, interactor, this);
        }
    }

    public string GetInteractionPrompt()
    {
        return $"{examinePrompt} {itemName}";
    }

    /// <summary>
    /// Called by ExamineSystem when player presses E while examining.
    /// Adds item to inventory.
    /// </summary>
    public void OnExamineConfirm()
    {
        isBeingExamined = false;
        InventoryManager.Instance.AddItem(itemName, itemIcon, gameObject);
        gameObject.SetActive(false);
        Debug.Log($"Added to inventory: {itemName}");
    }

    /// <summary>
    /// Called by ExamineSystem when player presses Q while examining.
    /// Drops item back to original position.
    /// </summary>
    public void OnExamineDrop()
    {
        isBeingExamined = false;
        Debug.Log($"Dropped: {itemName}");
    }
}