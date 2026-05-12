using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using System.Collections;

/// <summary>
/// Level 1 exit keypad — 3D physical panel with mesh buttons + dedicated camera view.
///
/// CAMERA:
///   - Optional PanelCameraSwitcher gives a fixed cinematic view of the panel.
///   - Falls back to FocusSystem if PanelCameraSwitcher isn't assigned.
///   - Exit with Escape or right-click.
/// </summary>
public class CodeEntryPanellevel1 : MonoBehaviour, IInteractable
{
    // ── Code Settings ─────────────────────────────────────────────────────────
    [Header("Code Settings")]
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private bool autoMatchLengthToCode = true;
    [SerializeField] private int codeLength = 4;

    // ── Scene Transition (Optional) ──────────────────────────────────────────
    [Header("Scene Transition (Optional)")]
    [SerializeField] private bool loadNextSceneOnComplete = false;
    [SerializeField] private string nextSceneName = "Level2";
    [SerializeField] private float sceneLoadDelay = 4.0f;
    [SerializeField] private CanvasGroup fadeOverlay;

    // ── 3D Screen Display ────────────────────────────────────────────────────
    [Header("3D Screen Display")]
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Color normalTextColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color correctTextColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color wrongTextColor = new Color(1f, 0f, 0f);

    [Tooltip("Optional: 'System Live' header text shown at top-left when idle.")]
    [SerializeField] private TMP_Text systemLiveText;
    [Tooltip("Optional: live time text shown at top-right.")]
    [SerializeField] private TMP_Text clockText;

    // ── HUD Toggle ───────────────────────────────────────────────────────────
    [Header("HUD Toggle")]
    [SerializeField] private GameObject crosshairUI;

    // ── Camera (preferred) ───────────────────────────────────────────────────
    [Header("Camera View")]
    [Tooltip("Optional dedicated camera for a clean fixed view of the panel.")]
    [SerializeField] private PanelCameraSwitcher cameraSwitcher;
    [Tooltip("Fallback if no PanelCameraSwitcher is set — uses your FocusSystem instead.")]
    [SerializeField] private bool useFocusSystemAsFallback = true;
    [SerializeField] private FocusSystem focusSystem;

    // ── Screen Shake ─────────────────────────────────────────────────────────
    [Header("Screen Shake on Wrong Entry")]
    [SerializeField] private bool screenShakeOnWrong = true;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    // ── Audio ────────────────────────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonPressSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.8f;

    // ── Events ───────────────────────────────────────────────────────────────
    [Header("Events")]
    public UnityEvent OnCodeCorrect;
    public UnityEvent OnCodeWrong;

    // ── State ────────────────────────────────────────────────────────────────
    private string currentInput = "";
    private InteractionSystem interactor;
    private bool isSolved = false;
    private bool isActive = false;
    private Camera playerCamera;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (autoMatchLengthToCode) codeLength = correctCode.Length;
        if (focusSystem == null && useFocusSystemAsFallback)
            focusSystem = FindFirstObjectByType<FocusSystem>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        if (statusText != null) statusText.text = "";
        if (systemLiveText != null) systemLiveText.text = "System Live";
        UpdateDisplay();
    }

    private void Update()
    {
        UpdateClock();

        if (!isActive || isSolved) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            ExitPanel();
    }

    private void UpdateClock()
    {
        if (clockText == null) return;
        clockText.text = System.DateTime.Now.ToString("HH:mm:ss");
    }

    // ── IInteractable ────────────────────────────────────────────────────────

    public void Interact(InteractionSystem interactionSystem)
    {
        if (isSolved || isActive) return;
        interactor = interactionSystem;
        playerCamera = Camera.main;
        isActive = true;

        if (crosshairUI != null) crosshairUI.SetActive(false);

        // Lock player movement & look, show cursor.
        interactor.EnterInteractionMode();

        // Preferred: dedicated panel camera.
        if (cameraSwitcher != null)
        {
            cameraSwitcher.SwitchToPanel();
        }
        else if (useFocusSystemAsFallback && focusSystem != null)
        {
            focusSystem.StartFocusing(gameObject);
            StartCoroutine(ForceCursorVisible());
        }
    }

    public string GetInteractionPrompt() => isSolved ? "" : "Enter Code";

    public bool IsActive => isActive;
    public bool IsSolved => isSolved;

    private IEnumerator ForceCursorVisible()
    {
        yield return null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ── Button Callbacks ─────────────────────────────────────────────────────

    public void PressNumber(string number)
    {
        if (!isActive || isSolved) return;
        if (currentInput.Length >= codeLength) return;
        currentInput += number;
        PlaySfx(buttonPressSound, 0.6f);
        UpdateDisplay();
    }

    public void PressClear()
    {
        if (!isActive || isSolved) return;
        currentInput = "";
        PlaySfx(buttonPressSound, 0.6f);
        UpdateDisplay();
        if (statusText != null) statusText.text = "";
        if (codeDisplay != null) codeDisplay.color = normalTextColor;
    }

    public void PressSubmit()
    {
        if (!isActive || isSolved) return;
        if (currentInput.Length < codeLength)
        {
            PlaySfx(buttonPressSound, 0.6f);
            return;
        }
        if (currentInput == correctCode) HandleCorrect();
        else HandleWrong();
    }

    // ── Results ──────────────────────────────────────────────────────────────

    private void HandleCorrect()
    {
        isSolved = true;
        if (codeDisplay != null) codeDisplay.color = correctTextColor;
        if (statusText != null)
        {
            statusText.text = "ACCESS GRANTED";
            statusText.color = correctTextColor;
        }
        PlaySfx(correctSound, 1f);
        OnCodeCorrect?.Invoke();

        StartCoroutine(ExitAfterDelay(0.8f));

        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
            StartCoroutine(LoadSceneAfterDelay());
    }

    private void HandleWrong()
    {
        if (codeDisplay != null) codeDisplay.color = wrongTextColor;
        if (statusText != null)
        {
            statusText.text = "ACCESS DENIED";
            statusText.color = wrongTextColor;
        }
        PlaySfx(wrongSound, 1f);
        OnCodeWrong?.Invoke();

        if (screenShakeOnWrong && playerCamera != null)
            StartCoroutine(ShakeCamera());

        currentInput = "";
        UpdateDisplay();
        Invoke(nameof(ResetDisplayColor), 0.9f);
    }

    private void ResetDisplayColor()
    {
        if (codeDisplay != null) codeDisplay.color = normalTextColor;
        if (statusText != null) statusText.text = "";
    }

    private IEnumerator ExitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ExitPanel();
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        if (fadeOverlay != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(t);
                yield return null;
            }
        }
        SceneManager.LoadScene(nextSceneName);
    }

    private void ExitPanel()
    {
        if (!isActive) return;
        isActive = false;

        if (crosshairUI != null && !isSolved) crosshairUI.SetActive(true);

        // Switch camera back.
        if (cameraSwitcher != null && cameraSwitcher.IsOnPanel)
            cameraSwitcher.SwitchToPlayer();
        else if (focusSystem != null && focusSystem.IsFocusing())
            focusSystem.ForceStopFocusing();

        if (interactor != null)
            interactor.ExitInteractionMode();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void UpdateDisplay()
    {
        if (codeDisplay == null) return;
        string display = "";
        for (int i = 0; i < codeLength; i++)
        {
            display += (i < currentInput.Length) ? currentInput[i].ToString() : "_";
            if (i < codeLength - 1) display += " ";
        }
        codeDisplay.text = display;
    }

    private void PlaySfx(AudioClip clip, float volumeScale)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    private IEnumerator ShakeCamera()
    {
        if (playerCamera == null) yield break;
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerCamera.transform.localPosition = originalPos;
    }
}