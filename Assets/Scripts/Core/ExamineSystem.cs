using UnityEngine;
using System.Collections;

public class ExamineSystem : MonoBehaviour
{

    [Header("Overlay")]
[SerializeField] private GameObject examineOverlay;

[SerializeField] private UnityEngine.Rendering.Volume postProcessVolume;


    [Header("Examine Settings")]
    // [SerializeField] private float examineDistance = 1.5f;
    [SerializeField] private float rotateSpeed = 5f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerMovement playerMovement;

    private GameObject currentObject;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private bool isExamining = false;
    private bool canDrop = false;

    private void Update()
    {
        if (!isExamining) return;

        Debug.Log("isExamining: " + isExamining + " canDrop: " + canDrop);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Rotate with right click
        if (Input.GetMouseButton(1))
{
    float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
    float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
    currentObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
    currentObject.transform.Rotate(Vector3.right, mouseY, Space.World);
    
    // Lock player look while rotating
    playerMovement.SetLookLocked(true);
}

        // Drop with E only when canDrop is true
        if (canDrop && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Dropping!");
            StopExamining();
        }
    }

    public void StartExamining(GameObject obj, InteractionSystem interactor)
    {
        if (isExamining) return;

        currentObject = obj;
        originalPosition = obj.transform.position;
        originalRotation = obj.transform.rotation;
        originalParent = obj.transform.parent;

        if (examineOverlay != null)
        examineOverlay.SetActive(true);

    if (postProcessVolume != null)
    postProcessVolume.weight = 1f;

        playerMovement.SetMovementLocked(true);
playerMovement.SetLookLocked(true);



        obj.transform.SetParent(playerCamera.transform);
        // obj.transform.localPosition = new Vector3(0, 0, examineDistance);
        obj.transform.localPosition = new Vector3(0, 0, 1.5f);
        obj.transform.localRotation = Quaternion.identity;

        isExamining = true;
        canDrop = false;

        // Enable dropping after 0.3 seconds using coroutine
        StartCoroutine(EnableDrop());
    }

    private IEnumerator EnableDrop()
    {
        yield return new WaitForSeconds(0.3f);
        canDrop = true;
        Debug.Log("canDrop is now TRUE");
    }

   public void StopExamining()
{
    if (!isExamining) return;

    currentObject.transform.SetParent(originalParent);
    currentObject.transform.position = originalPosition;
    currentObject.transform.rotation = originalRotation;

    // Temporarily disable collider to prevent immediate re-pickup
    Collider col = currentObject.GetComponent<Collider>();
    if (col != null)
    {
        col.enabled = false;
        StartCoroutine(ReEnableCollider(col));
    }

    if (postProcessVolume != null)
    postProcessVolume.weight = 0f;

    if (examineOverlay != null)
    examineOverlay.SetActive(false);


    isExamining = false;
    canDrop = false;
    playerMovement.SetMovementLocked(false);
    playerMovement.SetLookLocked(false);

    currentObject = null;
}

private IEnumerator ReEnableCollider(Collider col)
{
    yield return new WaitForSeconds(0.2f);
    if (col != null)
        col.enabled = true;
}

    public bool IsExamining() => isExamining;
}