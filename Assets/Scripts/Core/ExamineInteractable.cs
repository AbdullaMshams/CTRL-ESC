using UnityEngine;

public class ExamineInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Examine";
    [SerializeField] private string itemName = "Object";

    private ExamineSystem examineSystem;

    private void Start()
    {
        examineSystem = FindFirstObjectByType<ExamineSystem>();
    }

    public void Interact(InteractionSystem interactor)
    {
        if (examineSystem != null)
            examineSystem.StartExamining(gameObject, interactor);
    }

    public string GetInteractionPrompt()
    {
        return $"{promptText} {itemName}";
    }
}