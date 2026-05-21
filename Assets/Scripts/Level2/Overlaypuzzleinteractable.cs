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

    [Header("Snap Settings")]
    [SerializeField] private float snapDistance = 250f;
    [SerializeField] private Vector2 sheetASnapOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 sheetBSnapOffset = new Vector2(0f, 0f);

    [Header("Sounds")]
    [SerializeField] private AudioClip dragSound;   // paper rustling sound
    [SerializeField] private AudioClip snapSound;   // soft click/thud when snapped
    [SerializeField] private AudioClip openSound;   // sound when puzzle opens
    private AudioSource audioSource;

    private bool puzzleOpen = false;
    private bool sheetASnapped = false;
    private bool sheetBSnapped = false;
    private Canvas puzzleCanvas;

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

        // Set up audio source
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound

        sheetACurrentPos = sheetAStartPos;
        sheetBCurrentPos = sheetBStartPos;

        sheetA.anchoredPosition = sheetACurrentPos;
        sheetB.anchoredPosition = sheetBCurrentPos;

        puzzleCanvasRoot.SetActive(false);

        AddDragHandlers(sheetA, OnSheetADrop);
        AddDragHandlers(sheetB, OnSheetBDrop);
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

        // Play open sound
        if (openSound != null)
            audioSource.PlayOneShot(openSound);
    }

    private void ClosePuzzle()
    {
        puzzleOpen = false;

        sheetACurrentPos = sheetA.anchoredPosition;
        sheetBCurrentPos = sheetB.anchoredPosition;

        if (worldSpacePaper != null)
            worldSpacePaper.SetActive(true);

        puzzleCanvasRoot.SetActive(false);
        interactionSystem.ExitInteractionMode();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void AddDragHandlers(RectTransform sheet, System.Action onDrop)
    {
        EventTrigger trigger = sheet.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = sheet.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        // Begin drag — play drag sound
        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((data) =>
        {
            if (sheet == sheetA && sheetASnapped) return;
            if (sheet == sheetB && sheetBSnapped) return;
            if (dragSound != null)
                audioSource.PlayOneShot(dragSound);
        });
        trigger.triggers.Add(beginDragEntry);

        // Drag — follow mouse
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) =>
        {
            if (sheet == sheetA && sheetASnapped) return;
            if (sheet == sheetB && sheetBSnapped) return;

            PointerEventData ped = (PointerEventData)data;
            sheet.anchoredPosition += ped.delta / puzzleCanvas.scaleFactor;
        });
        trigger.triggers.Add(dragEntry);

        // End drag — check snap
        EventTrigger.Entry dropEntry = new EventTrigger.Entry();
        dropEntry.eventID = EventTriggerType.EndDrag;
        dropEntry.callback.AddListener((data) => onDrop());
        trigger.triggers.Add(dropEntry);

        // Click — unsnap
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerDown;
        clickEntry.callback.AddListener((data) =>
        {
            if (sheet == sheetA && sheetASnapped) sheetASnapped = false;
            if (sheet == sheetB && sheetBSnapped) sheetBSnapped = false;
        });
        trigger.triggers.Add(clickEntry);
    }

    private void OnSheetADrop()
    {
        float dist = Vector2.Distance(sheetA.anchoredPosition, basePaper.anchoredPosition);
        if (dist < snapDistance)
        {
            Vector2 target = basePaper.anchoredPosition + sheetASnapOffset;
            StartCoroutine(SmoothSnap(sheetA, target));
            sheetASnapped = true;
            sheetACurrentPos = target;

            // Play snap sound
            if (snapSound != null)
                audioSource.PlayOneShot(snapSound);
        }
    }

    private void OnSheetBDrop()
    {
        float dist = Vector2.Distance(sheetB.anchoredPosition, basePaper.anchoredPosition);
        if (dist < snapDistance)
        {
            Vector2 target = basePaper.anchoredPosition + sheetBSnapOffset;
            StartCoroutine(SmoothSnap(sheetB, target));
            sheetBSnapped = true;
            sheetBCurrentPos = target;

            // Play snap sound
            if (snapSound != null)
                audioSource.PlayOneShot(snapSound);
        }
    }

    private IEnumerator SmoothSnap(RectTransform sheet, Vector2 target)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        Vector2 start = sheet.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            sheet.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        sheet.anchoredPosition = target;
    }
}