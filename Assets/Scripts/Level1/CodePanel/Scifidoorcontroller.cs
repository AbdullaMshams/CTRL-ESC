using UnityEngine;
using System.Collections;

/// <summary>
/// Controls a sliding sci-fi double door, with optional handle/lever animation.
///
/// Open sequence on OpenDoor():
///   1. Handle pulls down + lever sound
///   2. Handle returns to rest position
///   3. Pause
///   4. Doors slide apart + door open sound
///
/// IMPORTANT: When using the SciFiDoorMask + SciFiDoorMasked shaders,
/// LEAVE hideDoorsAfterOpen = FALSE. The stencil mask handles "hiding"
/// the door pixels visually as they slide outside the frame — the GameObject
/// must stay active so the door geometry continues rendering correctly.
/// </summary>
public class SciFiDoorController : MonoBehaviour
{
    public enum SlideAxis { LocalX, LocalY, LocalZ }

    // ── Door References ──────────────────────────────────────────────────────
    [Header("Door Panels")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    // ── Slide Settings ───────────────────────────────────────────────────────
    [Header("Slide Settings")]
    [SerializeField] private SlideAxis slideAxis = SlideAxis.LocalX;
    [SerializeField] private float slideDistance = 1.5f;
    [SerializeField] private float slideDuration = 1.2f;
    [SerializeField] private bool invertLeftDirection = false;
    [SerializeField] private bool invertRightDirection = false;
    [SerializeField] private bool useSmoothing = true;

    [Header("Hide Doors After Open (LEGACY)")]
    [Tooltip("LEGACY behavior — SetActive(false) after sliding. " +
             "Leave OFF when using the stencil-mask shader (recommended). " +
             "The stencil will clip the doors visually as they pass the frame.")]
    [SerializeField] private bool hideDoorsAfterOpen = false;
    [SerializeField] private float hideDelay = 0.3f;

    // ── Handle (Optional) ────────────────────────────────────────────────────
    [Header("Handle / Lever Animation (Optional)")]
    [SerializeField] private Transform handle;
    [SerializeField] private float handleRotation = -75f;
    [SerializeField] private Vector3 handleRotationAxis = Vector3.right;
    [SerializeField] private float handlePullDuration = 0.4f;
    [SerializeField] private float handleHoldDuration = 0.25f;
    [SerializeField] private float handleReturnDuration = 0.35f;
    [SerializeField] private float pauseBeforeDoorSlide = 0.2f;

    // ── Audio ────────────────────────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip handleSound;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [Range(0f, 1f)][SerializeField] private float volume = 0.8f;

    // ── State ────────────────────────────────────────────────────────────────
    private Vector3 leftClosedPos, rightClosedPos;
    private Vector3 leftOpenPos, rightOpenPos;
    private Quaternion handleRestRot;
    private bool isOpen = false;
    private bool isAnimating = false;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("SciFiDoorController: Door references not set.", this);
            enabled = false;
            return;
        }

        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        Vector3 axis = GetAxisVector();
        leftOpenPos = leftClosedPos + axis * -slideDistance * (invertLeftDirection ? -1f : 1f);
        rightOpenPos = rightClosedPos + axis * slideDistance * (invertRightDirection ? -1f : 1f);

        if (handle != null) handleRestRot = handle.localRotation;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        StartCoroutine(OpenSequence());
        isOpen = true;
    }

    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        if (leftDoor != null) leftDoor.gameObject.SetActive(true);
        if (rightDoor != null) rightDoor.gameObject.SetActive(true);
        StartCoroutine(SlideDoors(leftOpenPos, rightOpenPos, leftClosedPos, rightClosedPos, closeSound));
        isOpen = false;
    }

    public void ToggleDoor()
    {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    // ── Sequencing ───────────────────────────────────────────────────────────

    private IEnumerator OpenSequence()
    {
        isAnimating = true;

        if (handle != null)
        {
            PlayClip(handleSound);

            Quaternion pulledRot = handleRestRot * Quaternion.AngleAxis(
                handleRotation, handleRotationAxis.normalized);

            yield return StartCoroutine(RotateHandle(handleRestRot, pulledRot, handlePullDuration));
            yield return new WaitForSeconds(handleHoldDuration);
            yield return StartCoroutine(RotateHandle(pulledRot, handleRestRot, handleReturnDuration));
            yield return new WaitForSeconds(pauseBeforeDoorSlide);
        }

        yield return StartCoroutine(SlideDoors(leftClosedPos, rightClosedPos,
                                               leftOpenPos, rightOpenPos, openSound));

        // LEGACY path — only used if you don't have the stencil shader set up.
        if (hideDoorsAfterOpen)
        {
            yield return new WaitForSeconds(hideDelay);
            if (leftDoor != null) leftDoor.gameObject.SetActive(false);
            if (rightDoor != null) rightDoor.gameObject.SetActive(false);
        }

        isAnimating = false;
    }

    private IEnumerator RotateHandle(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            handle.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        handle.localRotation = to;
    }

    private IEnumerator SlideDoors(Vector3 leftFrom, Vector3 rightFrom,
                                   Vector3 leftTo, Vector3 rightTo,
                                   AudioClip clip)
    {
        if (clip != null) PlayClip(clip);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            if (useSmoothing) t = Mathf.SmoothStep(0f, 1f, t);

            leftDoor.localPosition = Vector3.Lerp(leftFrom, leftTo, t);
            rightDoor.localPosition = Vector3.Lerp(rightFrom, rightTo, t);
            yield return null;
        }

        leftDoor.localPosition = leftTo;
        rightDoor.localPosition = rightTo;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void PlayClip(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    private Vector3 GetAxisVector()
    {
        switch (slideAxis)
        {
            case SlideAxis.LocalY: return Vector3.up;
            case SlideAxis.LocalZ: return Vector3.forward;
            case SlideAxis.LocalX:
            default: return Vector3.right;
        }
    }

    [ContextMenu("Test → Open Door")]
    private void TestOpen()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Enter Play Mode to test."); return; }
        OpenDoor();
    }

    [ContextMenu("Test → Close Door")]
    private void TestClose()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Enter Play Mode to test."); return; }
        CloseDoor();
    }
}