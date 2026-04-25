using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private string promptText = "Pick Up";

    public void Interact(InteractionSystem interactor)
    {
        bool added = InventoryManager.Instance.AddItem(itemName, itemIcon, gameObject);
        if (added)
        {
            gameObject.SetActive(false);
            Debug.Log($"Picked up: {itemName}");
        }
    }

    public string GetInteractionPrompt()
    {
        return $"{promptText} {itemName}";
    }
}