using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vault dial puzzle for Level 1 - Puzzle 3.
/// Player decodes a Morse message to find a 4-stage combination, then enters it on the dial.
///
/// MECHANIC (mimics a real combination safe):
///   Stage 1: turn dial RIGHT (clockwise) to first number
///   Stage 2: turn dial LEFT (counter-clockwise) to second number
///   Stage 3: turn dial RIGHT to third number
///   Stage 4: turn dial LEFT to fourth number
///
/// INPUT (both active):
///   - Mouse drag (click and drag in a circle around the dial)
///   - A key = rotate left, D key = rotate right (hold)
/// </summary>
public class VaultDialPuzzle : MonoBehaviour, IInteractable
{
    [Header("Combination")]
    [Tooltip("The 4 target numbers, each 0 to (positionsOnDial-1).")]
    [SerializeField] private int[] combination = new int[] { 12, 8, 25, 31 };

    [Tooltip("How many positions on the dial.")]
    [SerializeField] private int positionsOnDial = 40;

    [Range(0, 4)]
    [SerializeField] private int landingTolerance = 0;
    [SerializeField] private float landingDwellTime = 0.3f;
    [SerializeField] private int wrongDirectionTolerance = 3;

    [Header("Input")]
    [SerializeField] private float keyRotateSpeed = 100f;
    [SerializeField] private float mouseDragSensitivity = 2f;
    [Tooltip("Key that exits the dial puzzle without solving it.")]
    [SerializeField] private KeyCode exitKey = KeyCode.X;

    [Header("References")]
    [SerializeField] private Transform dial;
    [SerializeField] private SafeController safeController;
    private FocusSystem focusSystem;

    [Header("Dial Axis")]
    [SerializeField] private Vector3 dialRotationAxis = Vector3.forward;
    [SerializeField] private bool invertRotation = false;

    [Header("Audio")]
    [Tooltip("AudioSource for digit-lock, fail, and success sounds. Plays uninterrupted.")]
    [SerializeField] private AudioSource sfxAudioSource;
    [Tooltip("AudioSource ONLY for ticks. Gets interrupted on every new tick.")]
    [SerializeField] private AudioSource tickAudioSource;
    [SerializeField] private AudioClip tickSound;
    [SerializeField] private AudioClip digitLockSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private AudioClip successSound;
    [Range(0f, 1f)]
    [SerializeField] private float tickVolume = 0.4f;

    [Header("HUD")]
    [Tooltip("Root of the vault HUD canvas. Shown only while interacting with the dial.")]
    [SerializeField] private GameObject hudRoot;
    [Tooltip("The big live position number (00 - 39).")]
    [SerializeField] private TextMeshProUGUI currentPositionText;
    [Tooltip("Direction line: clockwise / counter-clockwise + stage indicator.")]
    [SerializeField] private TextMeshProUGUI directionText;

    [Header("HUD Slots (4 separate digit displays)")]
    [Tooltip("The 4 digit slot text fields, in order (slot 1 first).")]
    [SerializeField] private TextMeshProUGUI[] slotDigitTexts = new TextMeshProUGUI[4];
    [Tooltip("Optional: the 4 backgrounds/borders around each slot, so we can highlight the active one.")]
    [SerializeField] private Image[] slotBackgrounds = new Image[4];

    [Header("HUD Colors")]
    [SerializeField] private Color terminalGreen = new Color(0.247f, 1f, 0.498f);  // #3FFF7F
    [SerializeField] private Color terminalGreenDim = new Color(0.247f, 1f, 0.498f, 0.3f);
    [SerializeField] private Color landingColor = new Color(1f, 0.9f, 0.3f);
    [Tooltip("Background color for inactive/inset digit slot boxes.")]
    [SerializeField] private Color slotBoxColor = new Color(0.02f, 0.04f, 0.02f, 1f);

    [Header("Interaction Prompt")]
    [SerializeField] private string lockedPrompt = "Use Dial";
    [SerializeField] private string openedPrompt = "";

    private InteractionSystem interactor;
    private bool isActive = false;
    private bool isUnlocked = false;

    private float currentAngle = 0f;
    private float lastTickAngle = 0f;

    private int currentStage = 0;
    private int[] enteredDigits;
    private float dwellTimer = 0f;
    private float wrongDirectionAccumulator = 0f;

    private bool isDragging = false;
    private Vector2 dialScreenCenter;

    private int ExpectedDirection => (currentStage % 2 == 0) ? +1 : -1;
    private float DegreesPerPosition => 360f / positionsOnDial;

    private void Start()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
        }
        if (tickAudioSource == null)
        {
            tickAudioSource = gameObject.AddComponent<AudioSource>();
            tickAudioSource.playOnAwake = false;
        }

        if (focusSystem == null) focusSystem = FindFirstObjectByType<FocusSystem>();
        if (safeController == null) safeController = GetComponent<SafeController>();

        enteredDigits = new int[combination.Length];
        for (int i = 0; i < enteredDigits.Length; i++) enteredDigits[i] = -1;

        for (int i = 0; i < combination.Length; i++)
            combination[i] = Mathf.Clamp(combination[i], 0, positionsOnDial - 1);

        if (hudRoot != null) hudRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isActive || isUnlocked) return;

        if (Input.GetKeyDown(exitKey))
        {
            ExitDial();
            return;
        }

        HandleInput();
        CheckLanding();
        UpdateHUD();
    }

    public void Interact(InteractionSystem interactionSystem)
    {
        if (isUnlocked || isActive) return;

        interactor = interactionSystem;

        if (focusSystem != null)
            focusSystem.StartFocusing(gameObject);
        else
            interactor.EnterInteractionMode();

        isActive = true;
        currentStage = 0;
        dwellTimer = 0f;
        wrongDirectionAccumulator = 0f;
        lastTickAngle = currentAngle;
        for (int i = 0; i < enteredDigits.Length; i++) enteredDigits[i] = -1;

        if (hudRoot != null) hudRoot.SetActive(true);

        UpdateSlotDisplays();
        UpdateHUD();

        if (Camera.main != null && dial != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(dial.position);
            dialScreenCenter = new Vector2(screenPos.x, screenPos.y);
        }
    }

    public string GetInteractionPrompt() => isUnlocked ? openedPrompt : lockedPrompt;

    private void HandleInput()
    {
        float deltaAngle = 0f;

        if (Input.GetKey(KeyCode.A)) deltaAngle -= keyRotateSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) deltaAngle += keyRotateSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            if (Camera.main != null && dial != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(dial.position);
                dialScreenCenter = new Vector2(screenPos.x, screenPos.y);
            }
        }
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            Vector2 mouseNow = Input.mousePosition;
            Vector2 mousePrev = mouseNow - new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f;

            Vector2 vNow = mouseNow - dialScreenCenter;
            Vector2 vPrev = mousePrev - dialScreenCenter;

            if (vNow.sqrMagnitude > 100f && vPrev.sqrMagnitude > 100f)
            {
                float angleNow = Mathf.Atan2(vNow.y, vNow.x) * Mathf.Rad2Deg;
                float anglePrev = Mathf.Atan2(vPrev.y, vPrev.x) * Mathf.Rad2Deg;
                float diff = Mathf.DeltaAngle(anglePrev, angleNow);
                deltaAngle += -diff * mouseDragSensitivity;
            }
        }

        if (Mathf.Abs(deltaAngle) < 0.001f)
        {
            if (tickAudioSource != null && tickAudioSource.isPlaying)
                tickAudioSource.Stop();
            return;
        }
        if (invertRotation) deltaAngle = -deltaAngle;

        ApplyRotation(deltaAngle);
    }

    private void ApplyRotation(float deltaAngle)
    {
        currentAngle += deltaAngle;

        if (dial != null)
            dial.localRotation = Quaternion.AngleAxis(currentAngle, dialRotationAxis.normalized);

        float angleSinceLastTick = currentAngle - lastTickAngle;
        if (Mathf.Abs(angleSinceLastTick) >= DegreesPerPosition)
        {
            int ticks = Mathf.FloorToInt(Mathf.Abs(angleSinceLastTick) / DegreesPerPosition);
            PlayTickSound();
            lastTickAngle += DegreesPerPosition * ticks * Mathf.Sign(angleSinceLastTick);
        }

        int directionSign = deltaAngle > 0 ? +1 : -1;
        if (directionSign != ExpectedDirection)
        {
            wrongDirectionAccumulator += Mathf.Abs(deltaAngle) / DegreesPerPosition;
            if (wrongDirectionAccumulator >= wrongDirectionTolerance)
                ResetSequence(playSound: true);
        }
        else
        {
            wrongDirectionAccumulator = Mathf.Max(0,
                wrongDirectionAccumulator - Mathf.Abs(deltaAngle) / DegreesPerPosition * 0.5f);
        }

        if (Mathf.Abs(deltaAngle) > DegreesPerPosition * 0.1f) dwellTimer = 0f;
    }

    private void PlayTickSound()
    {
        if (tickSound == null || tickAudioSource == null) return;
        tickAudioSource.PlayOneShot(tickSound, tickVolume);
    }

    private int GetCurrentPosition()
    {
        float normalized = ((currentAngle % 360f) + 360f) % 360f;
        return Mathf.RoundToInt(normalized / DegreesPerPosition) % positionsOnDial;
    }

    private int DistanceToTarget()
    {
        if (currentStage >= combination.Length) return int.MaxValue;
        int current = GetCurrentPosition();
        int target = combination[currentStage];
        int diff = Mathf.Abs(current - target);
        return Mathf.Min(diff, positionsOnDial - diff);
    }

    private void CheckLanding()
    {
        if (currentStage >= combination.Length) return;

        if (DistanceToTarget() <= landingTolerance)
        {
            dwellTimer += Time.deltaTime;
            if (dwellTimer >= landingDwellTime) LockInDigit();
        }
        else
        {
            dwellTimer = 0f;
        }
    }

    private void LockInDigit()
    {
        enteredDigits[currentStage] = combination[currentStage];
        PlaySfx(digitLockSound, 0.9f);
        currentStage++;
        dwellTimer = 0f;
        wrongDirectionAccumulator = 0f;

        UpdateSlotDisplays();

        if (currentStage >= combination.Length) UnlockVault();
    }

    private void ResetSequence(bool playSound)
    {
        if (currentStage == 0) return;

        if (playSound) PlaySfx(failSound, 1f);
        currentStage = 0;
        dwellTimer = 0f;
        wrongDirectionAccumulator = 0f;

        for (int i = 0; i < enteredDigits.Length; i++) enteredDigits[i] = -1;
        UpdateSlotDisplays();
    }

    private void UpdateHUD()
    {
        if (currentPositionText != null)
        {
            int pos = GetCurrentPosition();
            currentPositionText.text = pos.ToString("00");
            currentPositionText.color = (DistanceToTarget() <= landingTolerance) ? landingColor : terminalGreen;
        }

        if (directionText != null)
        {
            if (currentStage < combination.Length)
            {
                string arrow = ExpectedDirection > 0 ? "↻" : "↺";
                string dirWord = ExpectedDirection > 0 ? "CLOCKWISE" : "COUNTER-CLOCKWISE";
                directionText.text = $"{arrow}  {dirWord}  //  STAGE {currentStage + 1} OF {combination.Length}";
                directionText.color = terminalGreen;
            }
            else
            {
                directionText.text = "UNLOCKED";
                directionText.color = landingColor;
            }
        }
    }

    /// <summary>
    /// Updates all 4 digit slot displays:
    /// - Locked-in slots show the entered digit in bright green
    /// - The current active slot has a brighter border
    /// - Future slots show "--" in dim green
    /// </summary>
    private void UpdateSlotDisplays()
    {
        for (int i = 0; i < slotDigitTexts.Length; i++)
        {
            if (slotDigitTexts[i] == null) continue;

            if (i < currentStage)
            {
                // Locked in
                slotDigitTexts[i].text = enteredDigits[i].ToString("00");
                slotDigitTexts[i].color = terminalGreen;
            }
            else
            {
                // Empty
                slotDigitTexts[i].text = "--";
                slotDigitTexts[i].color = terminalGreenDim;
            }
        }

        // Highlight the active slot's border
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] == null) continue;
            // Active slot gets a bright border by changing alpha; others stay dim
            slotBackgrounds[i].color = slotBoxColor;
        }
    }

    private void ExitDial()
    {
        isActive = false;
        if (hudRoot != null) hudRoot.SetActive(false);

        if (tickAudioSource != null && tickAudioSource.isPlaying)
            tickAudioSource.Stop();

        if (focusSystem != null)
            focusSystem.ForceStopFocusing();
        else if (interactor != null)
            interactor.ExitInteractionMode();
    }

    private void UnlockVault()
    {
        isUnlocked = true;
        isActive = false;

        if (hudRoot != null) hudRoot.SetActive(false);

        if (tickAudioSource != null && tickAudioSource.isPlaying)
            tickAudioSource.Stop();

        PlaySfx(successSound, 1f);

        if (safeController != null) safeController.OpenSafe();

        ExitDial();
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null || sfxAudioSource == null) return;
        sfxAudioSource.PlayOneShot(clip, volume);
    }
}