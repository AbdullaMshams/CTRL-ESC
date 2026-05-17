using UnityEngine;
using System.Collections;

public class SafeController : MonoBehaviour
{
    [Header("Parts to Animate")]
    public Transform dial;
    public Transform handle;
    public Transform door;

    [Header("Settings")]
    public float openAngle = 100f;
    public float animationSpeed = 2f;

    [Header("Safe Body Collider")]
    [SerializeField] private Collider safeBodyCollider;

    private bool isOpen = false;

    [ContextMenu("Open Safe")] // Allows you to test by right-clicking the component
    public void OpenSafe()
    {
        if (!isOpen)
        {
            StartCoroutine(AnimateOpening());
            isOpen = true;
        }
    }

    IEnumerator AnimateOpening()
    {
        // 1. Spin the Dial
        float elapsed = 0;
        while (elapsed < 1f)
        {
            dial.Rotate(Vector3.forward * 360 * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Turn the Handle
        elapsed = 0;
        Quaternion startHandleRot = handle.localRotation;
        Quaternion endHandleRot = startHandleRot * Quaternion.Euler(0, 0, -90);
        while (elapsed < 1f)
        {
            handle.localRotation = Quaternion.Slerp(startHandleRot, endHandleRot, elapsed);
            elapsed += Time.deltaTime * animationSpeed;
            yield return null;
        }

        // 3. Swing the Door
        elapsed = 0;
        Quaternion startDoorRot = door.localRotation;
        // Adjust the Y axis depending on how your model is oriented
        Quaternion endDoorRot = startDoorRot * Quaternion.Euler(0, openAngle, 0);
        while (elapsed < 1f)
        {
            door.localRotation = Quaternion.Slerp(startDoorRot, endDoorRot, elapsed);
            elapsed += Time.deltaTime * animationSpeed;
            yield return null;
        }
        // 4. Disable the safe body collider
        if (safeBodyCollider != null)
            safeBodyCollider.enabled = false;

        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
            col.enabled = false;

       
    }
}