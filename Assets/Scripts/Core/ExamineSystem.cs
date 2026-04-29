using UnityEngine;
using System.Collections;

public class ExamineSystem : MonoBehaviour
{
    [Header("Examine Settings")]
    [SerializeField] private float examineDistance = 1.5f;
    [SerializeField] private float rotateSpeed = 5f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private GameObject currentObject;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Vector3 originalScale;
    private bool isExamining = false;
    private bool canInteract = false;
    private PickupExamineItem currentPickupItem;

    private void Update()
    {
        if (!isExamining) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
            currentObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
            currentObject.transform.Rotate(Vector3.right, mouseY, Space.World);
        }

        if (!canInteract) return;

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

        obj.transform.localScale = originalScale * 0.4f;
        obj.transform.SetParent(playerCamera.transform);
        obj.transform.localPosition = new Vector3(0, 0, examineDistance);
        obj.transform.localRotation = Quaternion.identity;

        isExamining = true;
        canInteract = false;

        playerMovement.SetMovementLocked(true);
        playerMovement.SetLookLocked(true);

        StartCoroutine(EnableInteract());
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
            currentObject.transform.SetParent(null);
        }

        isExamining = false;
        canInteract = false;
        currentPickupItem = null;
        currentObject = null;

        playerMovement.SetMovementLocked(false);
        playerMovement.SetLookLocked(false);
    }

    private IEnumerator ReEnableCollider(Collider col)
    {
        yield return new WaitForSeconds(0.3f);
        if (col != null) col.enabled = true;
    }

    public bool IsExamining() => isExamining;
}