using UnityEngine;

/// <summary>
/// Example interactable object — use this as a template for all puzzles.
/// Attach to any GameObject to make it interactable.
/// Set the GameObject's Layer to "Interactable" in the Inspector.
/// </summary>
public class ExampleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Interact";

    public void Interact(InteractionSystem interactor)
    {
        // Replace this with your puzzle logic
        Debug.Log($"{gameObject.name} was interacted with!");

        // Example: If this puzzle needs a UI to open, call:
        // interactor.EnterInteractionMode();
        // Then when the UI closes, call:
        // interactor.ExitInteractionMode();
    }

    public string GetInteractionPrompt()
    {
        return promptText;
    }
}