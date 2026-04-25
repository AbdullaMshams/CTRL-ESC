using UnityEngine;

public class FocusInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Examine";
    private FocusSystem focusSystem;

    private void Start()
    {
        focusSystem = FindFirstObjectByType<FocusSystem>();
    }

    public void Interact(InteractionSystem interactor)
    {
        focusSystem.StartFocusing(gameObject);
    }

    public string GetInteractionPrompt()
    {
        return promptText;
    }
}