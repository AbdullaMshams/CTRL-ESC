using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the PosterAssemblyCanvas.
/// Tracks how many pieces are correctly placed.
/// When all 4 placed → hides grid → shows full poster + URL.
/// Press Escape to close the canvas.
/// </summary>
public class PosterAssemblyManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject leftPanel;          // Inventory pieces panel
    [SerializeField] private GameObject rightPanel;         // Assembly grid panel
    [SerializeField] private GameObject completedPosterPanel; // Shown when puzzle solved

    [Header("Completed Poster")]
    [SerializeField] private Image completedPosterImage;    // Full poster sprite
    [SerializeField] private TextMeshProUGUI urlText;       // URL revealed at end
    [SerializeField] private string portalURL = "portal.nexusdynamics.internal";

    [Header("Close Button (optional)")]
    [SerializeField] private GameObject closeButton;

    // Track progress
    private int placedCount   = 0;
    private const int TOTAL   = 4;
    private bool puzzleSolved = false;

    private void Start()
    {
        // Make sure completed panel is hidden at start
        if (completedPosterPanel != null)
            completedPosterPanel.SetActive(false);

        if (urlText != null)
            urlText.text = "";

        if (closeButton != null)
            closeButton.SetActive(false);
    }

    private void Update()
    {
        // Allow closing with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseCanvas();
    }

    /// <summary>
    /// Called by PosterSlot every time a piece is correctly placed.
    /// </summary>
    public void OnPiecePlaced()
    {
        placedCount++;
        Debug.Log($"Pieces placed: {placedCount}/{TOTAL}");

        if (placedCount >= TOTAL && !puzzleSolved)
        {
            puzzleSolved = true;
            RevealCompletedPoster();
        }
    }

    private void RevealCompletedPoster()
    {
        Debug.Log("Puzzle complete! Revealing full poster.");

        // Small delay before reveal for dramatic effect
        StartCoroutine(RevealWithDelay());
    }

    private System.Collections.IEnumerator RevealWithDelay()
    {
        yield return new WaitForSeconds(0.8f);

        // Hide the assembly panels
        if (leftPanel  != null) leftPanel.SetActive(false);
        if (rightPanel != null) rightPanel.SetActive(false);

        // Show completed poster panel
        if (completedPosterPanel != null)
            completedPosterPanel.SetActive(true);

        // Reveal URL text
        if (urlText != null)
            urlText.text = portalURL;

        // Show close button
        if (closeButton != null)
            closeButton.SetActive(true);

        Debug.Log($"Nexus Dynamics portal revealed: {portalURL}");
    }

    /// <summary>
    /// Closes the canvas and restores cursor lock for first-person movement.
    /// Hook this to your close button onClick too.
    /// </summary>
    public void CloseCanvas()
    {
        gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InteractionSystem interactor = FindFirstObjectByType<InteractionSystem>();
        if (interactor != null) interactor.ExitInteractionMode();

        // Also directly unlock the camera
        PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null)
        {
            pm.SetMovementLocked(false);
            pm.SetLookLocked(false);
        }
    }
}
