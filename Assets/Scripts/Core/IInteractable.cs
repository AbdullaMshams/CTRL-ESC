/// <summary>
/// Interface that ALL interactable objects in the game must implement.
/// 
/// How to use:
/// On any GameObject that the player should be able to interact with,
/// create a script and implement this interface.
/// 
/// Example:
///     public class Drawer : MonoBehaviour, IInteractable
///     {
///         public void Interact(InteractionSystem interactor) { ... open drawer ... }
///         public string GetInteractionPrompt() { return "Open Drawer"; }
///     }
/// 
/// Then set that GameObject's Layer to "Interactable" in Unity Inspector.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player presses E while looking at this object.
    /// </summary>
    /// <param name="interactor">Reference to the player's InteractionSystem</param>
    void Interact(InteractionSystem interactor);

    /// <summary>
    /// Returns the text shown in the interaction prompt.
    /// Example: "Open", "Pick Up", "Examine", "Read"
    /// </summary>
    string GetInteractionPrompt();
}