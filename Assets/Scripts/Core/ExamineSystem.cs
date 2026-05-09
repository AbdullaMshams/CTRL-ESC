using UnityEngine;
using System.Collections;

public class ExamineSystem : MonoBehaviour
{
    [Header("Examine Settings")]
    [SerializeField] private float rotateSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private float minZoomDistance = 0.5f;
    [SerializeField] private float maxZoomDistance = 2.5f;
    [SerializeField] private float zoomSpeed = 0.3f;
    [SerializeField] private float targetObjectSize = 0.15f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private GameObject currentObject;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Vector3 originalScale;
    private float currentZoomDistance;
    private bool isExamining = false;
    private bool canInteract = false;
    private PickupExamineItem currentPickupItem;

    private void Update()
    {
        if (!isExamining) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Rotate with right click
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
            currentObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
            currentObject.transform.Rotate(Vector3.right, mouseY, Space.World);
        }

        // Scroll to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoomDistance -= scroll * zoomSpeed;
            currentZoomDistance = Mathf.Clamp(
                currentZoomDistance, 
                minZoomDistance, 
                maxZoomDistance
            );
            currentObject.transform.localPosition = 
                new Vector3(0, 0, currentZoomDistance);
        }

        if (!canInteract) return;

        // E — add to inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentPickupItem != null)
            {
                currentPickupItem.OnExamineConfirm();
                StopExamining(false);
            }
            else
            {
                StopExamining(true);
            }
        }

        // Q — drop back
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentPickupItem != null)
                currentPickupItem.OnExamineDrop();
            StopExamining(true);
        }
    }

    public void StartExamining(GameObject obj, InteractionSystem interactor,
                               PickupExamineItem pickupItem = null)
    {
        if (isExamining) return;

        currentObject = obj;
        currentPickupItem = pickupItem;
        originalPosition = obj.transform.position;
        originalRotation = obj.transform.rotation;
        originalParent = obj.transform.parent;
        originalScale = obj.transform.localScale;

        // Calculate dynamic scale based on object bounds
        float objectSize = GetObjectSize(obj);
        float scaleFactor = targetObjectSize / objectSize;
        obj.transform.localScale = originalScale * scaleFactor;

        // Set initial zoom distance
        currentZoomDistance = maxZoomDistance * 0.4f;

        // Move in front of camera
        obj.transform.SetParent(playerCamera.transform);
        obj.transform.localPosition = new Vector3(0, 0, currentZoomDistance);
        obj.transform.localRotation = Quaternion.identity;

        isExamining = true;
        canInteract = false;

        playerMovement.SetMovementLocked(true);
        playerMovement.SetLookLocked(true);

        StartCoroutine(EnableInteract());
    }

    private float GetObjectSize(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 1f;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        // Return the largest dimension
        return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    }

    private IEnumerator EnableInteract()
    {
        yield return null;
        yield return new WaitForSeconds(0.3f);
        canInteract = true;
        Debug.Log("canInteract is now TRUE");
    }

    private void StopExamining(bool returnToPlace)
{
    if (!isExamining) return;

    if (currentObject != null)
        currentObject.transform.localScale = originalScale;

    if (returnToPlace && currentObject != null)
    {
        currentObject.transform.SetParent(originalParent);
        currentObject.transform.position = originalPosition;
        currentObject.transform.rotation = originalRotation;

        Collider col = currentObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(ReEnableCollider(col));
        }
    }
    else if (currentObject != null)
    {
        // Unparent and drop in front of player
        currentObject.transform.SetParent(null);

        // Position in front of player at waist height
        Vector3 dropPos = playerCamera.transform.position 
            + playerCamera.transform.forward * 0.8f;
        dropPos.y -= 0.3f;
        currentObject.transform.position = dropPos;

        // Add rigidbody for drop physics
        Rigidbody rb = currentObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = currentObject.AddComponent<Rigidbody>();
        
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        // Disable collider briefly then re-enable
        Collider col = currentObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            StartCoroutine(ReEnableCollider(col));
        }

        // Remove rigidbody after it lands
        StartCoroutine(RemoveRigidbody(rb));
    }

    isExamining = false;
    canInteract = false;
    currentPickupItem = null;
    currentObject = null;

    playerMovement.SetMovementLocked(false);
    playerMovement.SetLookLocked(false);
}

private IEnumerator RemoveRigidbody(Rigidbody rb)
{
    // Wait until it stops moving
    yield return new WaitForSeconds(2f);
    if (rb != null)
        Destroy(rb);
}

private IEnumerator ReEnableCollider(Collider col)
{
    yield return new WaitForSeconds(0.8f);
    if (col != null)
        col.enabled = true;
}

    // private IEnumerator ReEnableCollider(Collider col)
    // {
    //     yield return new WaitForSeconds(0.3f);
    //     if (col != null) col.enabled = true;
    // }

    public bool IsExamining() => isExamining;
}