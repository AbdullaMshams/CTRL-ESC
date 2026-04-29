using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeEntryPanel : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private GameObject door;

    [Header("UI References")]
    [SerializeField] private GameObject codeEntryCanvas;
    [SerializeField] private TextMeshProUGUI codeDisplay;

    [Header("Feedback Colors")]
    [SerializeField] private Image panelImage;
    [SerializeField] private Color normalColor = new Color(0.08f, 0.08f, 0.08f, 0.94f);
    [SerializeField] private Color correctColor = new Color(0f, 0.6f, 0f, 0.94f);
    [SerializeField] private Color wrongColor = new Color(0.6f, 0f, 0f, 0.94f);

    private string currentInput = "";
    private InteractionSystem interactor;
    private bool isSolved = false;

    private void Start()
    {
        codeEntryCanvas.SetActive(false);
    }

    // Called when player presses E on this object
    public void Interact(InteractionSystem interactionSystem)
    {
        if (isSolved) return;
        interactor = interactionSystem;
        codeEntryCanvas.SetActive(true);
        interactor.EnterInteractionMode();
        UpdateDisplay();
    }

    public string GetInteractionPrompt()
    {
        return isSolved ? "" : "Enter Code";
    }

    // Called by each number button
    public void PressNumber(string number)
    {
        if (currentInput.Length >= 4) return;
        currentInput += number;
        UpdateDisplay();
    }

    // Called by Clear button
    public void PressClear()
    {
        currentInput = "";
        UpdateDisplay();
        if (panelImage != null)
            panelImage.color = normalColor;
    }

    // Called by Submit button
    public void PressSubmit()
    {
        if (currentInput == correctCode)
        {
            // Correct!
            if (panelImage != null)
                panelImage.color = correctColor;
            isSolved = true;

            // Open the door
            if (door != null)
                door.SetActive(false);

            // Close panel after short delay
            Invoke(nameof(ClosePanel), 1f);
        }
        else
        {
            // Wrong!
            if (panelImage != null)
                panelImage.color = wrongColor;
            currentInput = "";
            Invoke(nameof(ResetColor), 0.8f);
            UpdateDisplay();
        }
    }

    private void ResetColor()
    {
        if (panelImage != null)
            panelImage.color = normalColor;
        UpdateDisplay();
    }

    private void ClosePanel()
    {
        codeEntryCanvas.SetActive(false);
        if (interactor != null)
            interactor.ExitInteractionMode();
    }

    // Close panel with Escape key
    private void Update()
    {
        if (codeEntryCanvas.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    private void UpdateDisplay()
    {
        string display = "";
        for (int i = 0; i < 4; i++)
        {
            if (i < currentInput.Length)
                display += currentInput[i];
            else
                display += "_";

            if (i < 3) display += " ";
        }
        codeDisplay.text = display;
    }
}