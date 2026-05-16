using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class OverlayPuzzleInteractable : MonoBehaviour, IInteractable
{
    [Header("Core References")]
    [SerializeField] private InteractionSystem interactionSystem;

    [Header("World Space Paper (hides when puzzle opens)")]
    [SerializeField] private GameObject worldSpacePaper;

    [Header("Puzzle Canvas")]
    [SerializeField] private GameObject puzzleCanvasRoot;
    [SerializeField] private RectTransform basePaper;

    [Header("Sheets")]
    [SerializeField] private RectTransform sheetA;
    [SerializeField] private RectTransform sheetB;

    [Header("Sheet Start Positions")]
    [SerializeField] private Vector2 sheetAStartPos = new Vector2(-600f, 0f);
    [SerializeField] private Vector2 sheetBStartPos = new Vector2(600f, 0f);

    [Header("Snap Settings")]
    [SerializeField] private float snapDistance = 150f;

    [Header("Code Entry")]
    [SerializeField] private GameObject codeEntryUI;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Completion")]
    [SerializeField] private GameObject electricBoxLock;
    [SerializeField] private string correctCode = "72594";

    private bool puzzleOpen = false;
    private bool sheetASnapped = false;
    private bool sheetBSnapped = false;
    private bool puzzleSolved = false;
    private Canvas puzzleCanvas;

    public string GetInteractionPrompt() => puzzleSolved ? "" : "Examine Document";

    public void Interact(InteractionSystem interactor)
    {
        if (puzzleSolved) return;
        OpenPuzzle();
    }

    private void Start()
    {
        if (interactionSystem == null)
            interactionSystem = FindFirstObjectByType<InteractionSystem>();

        puzzleCanvas = puzzleCanvasRoot.GetComponent<Canvas>();

        StyleCodeEntryUI();

        puzzleCanvasRoot.SetActive(false);
        codeEntryUI.SetActive(false);
        feedbackText.text = "";

        sheetA.anchoredPosition = sheetAStartPos;
        sheetB.anchoredPosition = sheetBStartPos;

        submitButton.onClick.AddListener(OnSubmitCode);

        AddDragHandlers(sheetA, OnSheetADrop);
        AddDragHandlers(sheetB, OnSheetBDrop);
    }

    private void StyleCodeEntryUI()
    {
        // Input field background
        Image inputBg = codeInputField.GetComponent<Image>();
        if (inputBg != null)
            inputBg.color = new Color(0f, 0.05f, 0f, 0.95f);

        // Input text
        codeInputField.textComponent.color = new Color(0f, 1f, 0.25f);
        codeInputField.textComponent.fontSize = 28;
        codeInputField.textComponent.alignment = TextAlignmentOptions.Center;
        codeInputField.textComponent.fontStyle = FontStyles.Bold;

        // Placeholder
        if (codeInputField.placeholder is TextMeshProUGUI ph)
        {
            ph.text = "_ _ _ _ _";
            ph.color = new Color(0f, 0.4f, 0.1f, 0.8f);
            ph.alignment = TextAlignmentOptions.Center;
            ph.fontSize = 24;
        }

        // Button background
        Image btnBg = submitButton.GetComponent<Image>();
        if (btnBg != null)
            btnBg.color = new Color(0f, 0.08f, 0f, 0.95f);

        // Button text
        TextMeshProUGUI btnText = submitButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = "SUBMIT";
            btnText.color = new Color(0f, 0.8f, 0.2f);
            btnText.fontSize = 20;
            btnText.fontStyle = FontStyles.Bold;
            btnText.characterSpacing = 4f;
        }

        // Feedback text
        feedbackText.fontSize = 18;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.characterSpacing = 3f;
    }

    private void Update()
    {
        if (!puzzleOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            ClosePuzzle();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnSubmitCode();
    }

    private void OpenPuzzle()
    {
        puzzleOpen = true;

        // Hide world space paper on the table
        if (worldSpacePaper != null)
            worldSpacePaper.SetActive(false);

        puzzleCanvasRoot.SetActive(true);
        interactionSystem.EnterInteractionMode();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ClosePuzzle()
    {
        puzzleOpen = false;

        // Show world space paper again
        if (worldSpacePaper != null)
            worldSpacePaper.SetActive(true);

        puzzleCanvasRoot.SetActive(false);
        interactionSystem.ExitInteractionMode();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        feedbackText.text = "";

        // Reset puzzle state
        ResetPuzzle();
    }

    private void ResetPuzzle()
    {
        // Reset sheets to starting positions
        sheetA.anchoredPosition = sheetAStartPos;
        sheetB.anchoredPosition = sheetBStartPos;

        // Reset snap state
        sheetASnapped = false;
        sheetBSnapped = false;

        // Hide code entry
        codeEntryUI.SetActive(false);
        codeInputField.text = "";
        feedbackText.text = "";
    }

    private void AddDragHandlers(RectTransform sheet, System.Action onDrop)
    {
        EventTrigger trigger = sheet.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = sheet.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) =>
        {
            PointerEventData ped = (PointerEventData)data;
            sheet.anchoredPosition += ped.delta / puzzleCanvas.scaleFactor;
        });
        trigger.triggers.Add(dragEntry);

        EventTrigger.Entry dropEntry = new EventTrigger.Entry();
        dropEntry.eventID = EventTriggerType.EndDrag;
        dropEntry.callback.AddListener((data) => onDrop());
        trigger.triggers.Add(dropEntry);
    }

    private void OnSheetADrop()
    {
        if (sheetASnapped) return;
        float dist = Vector2.Distance(sheetA.anchoredPosition, basePaper.anchoredPosition);
        if (dist < snapDistance)
        {
            sheetA.anchoredPosition = basePaper.anchoredPosition;
            sheetASnapped = true;
            CheckBothSnapped();
        }
    }

    private void OnSheetBDrop()
    {
        if (sheetBSnapped) return;
        float dist = Vector2.Distance(sheetB.anchoredPosition, basePaper.anchoredPosition);
        if (dist < snapDistance)
        {
            sheetB.anchoredPosition = basePaper.anchoredPosition + new Vector2(2f, -2f);
            sheetBSnapped = true;
            CheckBothSnapped();
        }
    }

    private void CheckBothSnapped()
    {
        if (sheetASnapped && sheetBSnapped)
            StartCoroutine(ShowCodeEntryDelay());
    }

    private IEnumerator ShowCodeEntryDelay()
    {
        yield return new WaitForSeconds(0.4f);
        codeEntryUI.SetActive(true);
        codeInputField.Select();
        codeInputField.ActivateInputField();
    }

    private void OnSubmitCode()
    {
        string entered = codeInputField.text.Trim();
        if (entered == correctCode)
            StartCoroutine(SolveSequence());
        else
            StartCoroutine(WrongCodeFlash());
    }

    private IEnumerator SolveSequence()
    {
        feedbackText.color = new Color(0f, 1f, 0.25f);
        feedbackText.text = "ACCESS GRANTED";
        puzzleSolved = true;

        yield return new WaitForSeconds(1.5f);

        if (electricBoxLock != null)
            electricBoxLock.SetActive(false);

        ClosePuzzle();
    }

    private IEnumerator WrongCodeFlash()
    {
        feedbackText.color = new Color(1f, 0.2f, 0.2f);
        feedbackText.text = "ACCESS DENIED";

        Vector2 original = codeInputField.GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i < 6; i++)
        {
            codeInputField.GetComponent<RectTransform>().anchoredPosition =
                original + new Vector2(i % 2 == 0 ? -8f : 8f, 0);
            yield return new WaitForSeconds(0.05f);
        }
        codeInputField.GetComponent<RectTransform>().anchoredPosition = original;

        yield return new WaitForSeconds(0.8f);
        feedbackText.text = "";
        codeInputField.text = "";
    }
}