using UnityEngine;
using System.Collections;

public class FocusSystem : MonoBehaviour
{
    [Header("Focus Settings")]
    [SerializeField] private float focusSpeed = 3f;
    [SerializeField] private float focusDistance = 1.2f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isFocusing = false;
    private bool canExit = false;
    private Transform focusTarget;

    private void Update()
    {
        if (!isFocusing) return;

        if (!canExit) return;

        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            StopFocusing();
        }
    }

    public void StartFocusing(GameObject target)
    {
        if (isFocusing) return;

        focusTarget = target.transform;
        originalCameraPosition = playerCamera.transform.localPosition;
        originalCameraRotation = playerCamera.transform.localRotation;

        isFocusing = true;
        canExit = false;

        playerMovement.SetMovementLocked(true);
        playerMovement.SetLookLocked(true);

        StartCoroutine(FocusCoroutine());
    }

private IEnumerator FocusCoroutine()
{
    // Get direction but keep camera at its current height
    Vector3 directionToObject = focusTarget.position - playerCamera.transform.position;
    directionToObject.y = 0; // ignore vertical difference
    directionToObject.Normalize();

    // Target position — same height as camera, just moved forward toward object
    Vector3 targetWorldPos = focusTarget.position 
        - directionToObject * focusDistance;
    targetWorldPos.y = playerCamera.transform.position.y; // keep camera height

    // Look toward the object center
    Quaternion targetRotation = Quaternion.LookRotation(
        focusTarget.position - targetWorldPos);

    float elapsed = 0f;
    float duration = 0.6f;

    Vector3 startPos = playerCamera.transform.position;
    Quaternion startRot = playerCamera.transform.rotation;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, elapsed / duration);
        playerCamera.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
        playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
        yield return null;
    }

    yield return new WaitForSeconds(0.3f);
    canExit = true;
}

    private void StopFocusing()
    {
        if (!isFocusing) return;
        StartCoroutine(UnfocusCoroutine());
    }

    private IEnumerator UnfocusCoroutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        // Convert original local position to world position
        Vector3 targetWorldPos = playerCamera.transform.parent
            .TransformPoint(originalCameraPosition);
        Quaternion targetWorldRot = playerCamera.transform.parent.rotation 
            * originalCameraRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            playerCamera.transform.position = Vector3.Lerp(startPos, 
                targetWorldPos, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startRot, 
                targetWorldRot, t);
            yield return null;
        }

        // Restore local transform cleanly
        playerCamera.transform.localPosition = originalCameraPosition;
        playerCamera.transform.localRotation = originalCameraRotation;

        isFocusing = false;
        canExit = false;
        focusTarget = null;

        playerMovement.SetMovementLocked(false);
        playerMovement.SetLookLocked(false);
    }
    public void ForceStopFocusing()
    {
        StopAllCoroutines();
        StartCoroutine(UnfocusCoroutine());
    }
    public bool IsFocusing() => isFocusing;
}