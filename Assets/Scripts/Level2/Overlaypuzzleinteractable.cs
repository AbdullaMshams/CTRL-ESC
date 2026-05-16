using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    [SerializeField] private Vector2 sheetAStartPos = new Vector2(-700f, 0f);
    [SerializeField] private Vector2 sheetBStartPos = new Vector2(700f, 0f);

    private bool puzzleOpen = false;
    private Canvas puzzleCanvas;

    // Persist positions between opens
    private Vector2 sheetACurrentPos;
    private Vector2 sheetBCurrentPos;

    public string GetInteractionPrompt() => "Examine Document";

    public void Interact(InteractionSystem interactor)
    {
        if (puzzleOpen) return;
        OpenPuzzle();
    }

    private void Start()
    {
        if (interactionSystem == null)
            interactionSystem = FindFirstObjectByType<InteractionSystem>();

        puzzleCanvas = puzzleCanvasRoot.GetComponent<Canvas>();

        sheetACurrentPos = sheetAStartPos;
        sheetBCurrentPos = sheetBStartPos;

        sheetA.anchoredPosition = sheetACurrentPos;
        sheetB.anchoredPosition = sheetBCurrentPos;

        puzzleCanvasRoot.SetActive(false);

        AddDragHandlers(sheetA);
        AddDragHandlers(sheetB);
    }

    private void Update()
    {
        if (!puzzleOpen) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            ClosePuzzle();
    }

    private void OpenPuzzle()
    {
        puzzleOpen = true;

        if (worldSpacePaper != null)
            worldSpacePaper.SetActive(false);

        sheetA.anchoredPosition = sheetACurrentPos;
        sheetB.anchoredPosition = sheetBCurrentPos;

        puzzleCanvasRoot.SetActive(true);
        interactionSystem.EnterInteractionMode();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ClosePuzzle()
    {
        puzzleOpen = false;

        // Save current positions so they stay when reopened
        sheetACurrentPos = sheetA.anchoredPosition;
        sheetBCurrentPos = sheetB.anchoredPosition;

        if (worldSpacePaper != null)
            worldSpacePaper.SetActive(true);

        puzzleCanvasRoot.SetActive(false);
        interactionSystem.ExitInteractionMode();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void AddDragHandlers(RectTransform sheet)
    {
        EventTrigger trigger = sheet.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = sheet.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        // Free drag — no snapping, no locking
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) =>
        {
            PointerEventData ped = (PointerEventData)data;
            sheet.anchoredPosition += ped.delta / puzzleCanvas.scaleFactor;
        });
        trigger.triggers.Add(dragEntry);
    }
}