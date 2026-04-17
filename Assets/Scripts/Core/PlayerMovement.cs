using UnityEngine;

/// <summary>
/// Handles first-person player movement, mouse look, and crouching.
/// Attach this script to the Player GameObject.
/// The Player GameObject should have a CharacterController component.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float crouchSpeed = 1.5f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    // Private variables
    private CharacterController characterController;
    private float verticalRotation = 0f;
    private float currentHeight;
    private bool isCrouching = false;
    private bool isMovementLocked = false;
    private bool isLookLocked = false;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock and hide cursor on start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set initial height
        currentHeight = standingHeight;
        characterController.height = standingHeight;

        // Auto find camera if not assigned
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        // Do nothing if movement and look are locked
        // (used when player is interacting with puzzles)
        if (!isMovementLocked)
            HandleMovement();

        if (!isLookLocked)
            HandleMouseLook();

        HandleCrouch();
    }

    /// <summary>
    /// Handles WASD movement using CharacterController.
    /// </summary>
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical   = Input.GetAxis("Vertical");   // W/S

        float speed = isCrouching ? crouchSpeed : walkSpeed;

        // Move relative to where the player is facing
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection *= speed;

        // Apply gravity
        moveDirection.y = -9.81f;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Handles mouse look — horizontal rotates the player body,
    /// vertical rotates the camera up/down only.
    /// </summary>
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player body left/right
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down (clamped)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    /// <summary>
    /// Handles crouching by smoothly transitioning CharacterController height.
    /// Press Left Control to toggle crouch.
    /// </summary>
    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = !isCrouching;

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        characterController.height = currentHeight;

        // Move camera with crouch
        Vector3 camPos = playerCamera.transform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, isCrouching ? 0.2f : 0.7f, crouchTransitionSpeed * Time.deltaTime);
        playerCamera.transform.localPosition = camPos;
    }

    // ── Public Methods ────────────────────────────────────────────────────────
    // Called by InteractionSystem when player starts/stops interacting with a puzzle

    /// <summary>
    /// Lock or unlock player movement.
    /// Call this when opening a puzzle UI.
    /// </summary>
    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;
    }

    /// <summary>
    /// Lock or unlock mouse look.
    /// Call this when opening a puzzle UI.
    /// </summary>
    public void SetLookLocked(bool locked)
    {
        isLookLocked = locked;

        if (locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Returns true if player is currently crouching.
    /// </summary>
    public bool IsCrouching() => isCrouching;
}