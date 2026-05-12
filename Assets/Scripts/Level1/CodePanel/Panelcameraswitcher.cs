using UnityEngine;
using System.Collections;

/// <summary>
/// Switches between the player's first-person camera and a dedicated panel camera
/// positioned in front of the keypad. Gives a clean cinematic view that's
/// guaranteed to frame the panel perfectly every time.
///
/// HOW IT WORKS:
///   - On Interact, disables the player camera, enables the panel camera.
///   - On Exit, swaps them back.
///   - Player movement/look stay locked while in panel view (cursor visible).
///
/// HOW TO SET UP:
///   1. In your scene, create a new empty GameObject as a CHILD of the CodePanel.
///   2. Name it "PanelCamera". Add Component → Camera.
///   3. Position it directly in front of the keypad — frame the keypad perfectly
///      in the Game view by entering the camera's view (Camera Preview window).
///   4. Disable the Camera component for now (untick it in inspector).
///   5. Set the AudioListener component on it to disabled (so you don't get
///      a duplicate AudioListener warning).
///   6. Drag this PanelCamera into "panelCamera" field on CodeEntryPanel.
/// </summary>
public class PanelCameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera panelCamera;

    [Header("Transition")]
    [Tooltip("If true, smoothly lerps between the two camera positions instead of an instant cut.")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionDuration = 0.5f;

    private bool isOnPanel = false;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (panelCamera != null) panelCamera.gameObject.SetActive(false);
    }

    public void SwitchToPanel()
    {
        if (isOnPanel || panelCamera == null) return;

        panelCamera.gameObject.SetActive(true);

        isOnPanel = true;

        if (smoothTransition)
            StartCoroutine(TransitionTo(panelCamera, playerCamera));
        else
            HardSwitch(panelCamera, playerCamera);
    }

    public void SwitchToPlayer()
    {
        if (!isOnPanel || panelCamera == null) return;
        isOnPanel = false;

        if (smoothTransition)
            StartCoroutine(TransitionTo(playerCamera, panelCamera));
        else
            HardSwitch(playerCamera, panelCamera);
    }

    public bool IsOnPanel => isOnPanel;

    // ── Internal ─────────────────────────────────────────────────────────────

    private void HardSwitch(Camera enable, Camera disable)
    {
        if (enable != null) enable.gameObject.SetActive(true);
        if (disable != null) disable.gameObject.SetActive(false);
    }

    private IEnumerator TransitionTo(Camera target, Camera from)
    {
        // We use a temp "blend camera" approach: move the "from" camera toward
        // the target's position/rotation, then snap to the actual target camera.
        if (target == null || from == null) yield break;

        Vector3 startPos = from.transform.position;
        Quaternion startRot = from.transform.rotation;
        Vector3 endPos = target.transform.position;
        Quaternion endRot = target.transform.rotation;

        // Enable target invisibly first (so it can render the next frame).
        target.gameObject.SetActive(true);
        target.enabled = false;
        from.enabled = true;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);
            from.transform.position = Vector3.Lerp(startPos, endPos, t);
            from.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // Snap to target and disable the from-camera.
        from.transform.position = startPos;
        from.transform.rotation = startRot;
        from.enabled = false;
        from.gameObject.SetActive(false);

        target.enabled = true;
    }
}