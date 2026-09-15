using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlanetaryWalkerController : NetworkBehaviour
{
    public enum MovementState { PlanetBound, ZeroG }

    [Header("State Management")]
    public MovementState currentState = MovementState.PlanetBound;

    [Header("Movement Settings")]
    public float movementspeed = 10.0f;
    public float zeroGSpeedMultiplier = 1.5f;
    public float jumpHeight = 5.0f;
    public bool canJump = true;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public Transform cameraPivot;
    public float CameraSpeed = 2.0f;
    public float CameraXLimit = 85.0f;

    private Rigidbody rb;
    private PlanetGravity planetGravity;
    private InputSystem_Actions input;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool grounded;
    private bool jumpPressed;
    private bool crouchPressed;

    private float xRotation = 0f;
    private float VelocityChangeLimit = 10.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        planetGravity = GetComponent<PlanetGravity>();

        rb.freezeRotation = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearDamping = 1f;
    }

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        if (!isOwner)
        {
            if (playerCamera != null)
                playerCamera.enabled = false;

            enabled = false;
            return;
        }

        input = new InputSystem_Actions();

        input.Player.Move.performed += OnMovePerformed;
        input.Player.Move.canceled += OnMoveCanceled;

        input.Player.Look.performed += OnLookPerformed;
        input.Player.Look.canceled += OnLookCanceled;

        input.Player.Jump.performed += OnJumpPerformed;

        input.Player.Crouch.performed += OnCrouchPerformed;
        input.Player.Crouch.canceled += OnCrouchCanceled;

        input.Player.Enable();

        if (playerCamera != null)
            playerCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        DisableInput();
    }

    private void OnDestroy()
    {
        DisableInput();
    }

    private void DisableInput()
    {
        if (input == null)
            return;

        input.Player.Move.performed -= OnMovePerformed;
        input.Player.Move.canceled -= OnMoveCanceled;

        input.Player.Look.performed -= OnLookPerformed;
        input.Player.Look.canceled -= OnLookCanceled;

        input.Player.Jump.performed -= OnJumpPerformed;

        input.Player.Crouch.performed -= OnCrouchPerformed;
        input.Player.Crouch.canceled -= OnCrouchCanceled;

        input.Player.Disable();
        input.UI.Disable();

        input.Dispose();
        input = null;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext context)
    {
        crouchPressed = true;
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        crouchPressed = false;
    }

    private void Update()
    {
        if (!isOwner)
            return;

        HandleCamera();
    }

    private void HandleCamera()
    {
        float mouseX = lookInput.x * CameraSpeed;
        float mouseY = lookInput.y * CameraSpeed;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -CameraXLimit, CameraXLimit);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        if (!isOwner)
            return;

        switch (currentState)
        {
            case MovementState.PlanetBound:
                HandlePlanetMovement();
                break;

            case MovementState.ZeroG:
                HandleZeroGMovement();
                break;
        }

        jumpPressed = false;
        grounded = false;
    }

    private void HandlePlanetMovement()
    {
        if (!grounded)
            return;

        Vector3 forwardDir = Vector3.Cross(transform.right, transform.up).normalized;
        Vector3 rightDir = Vector3.Cross(transform.up, transform.forward).normalized;

        Vector3 targetVelocity = (forwardDir * moveInput.y + rightDir * moveInput.x) * movementspeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 charRelativeVelocity = transform.InverseTransformDirection(velocity);
        charRelativeVelocity.y = 0;

        Vector3 velocityChange = transform.InverseTransformDirection(targetVelocity) - charRelativeVelocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -VelocityChangeLimit, VelocityChangeLimit);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -VelocityChangeLimit, VelocityChangeLimit);
        velocityChange.y = 0;

        rb.AddForce(transform.TransformDirection(velocityChange), ForceMode.VelocityChange);

        if (jumpPressed && canJump)
            rb.AddForce(transform.up * jumpHeight, ForceMode.VelocityChange);
    }

    private void HandleZeroGMovement()
    {
        Vector3 zeroGInput = new Vector3(moveInput.x, 0f, moveInput.y);

        if (jumpPressed)
            zeroGInput.y = 1f;
        else if (crouchPressed)
            zeroGInput.y = -1f;

        Vector3 moveDirection = playerCamera.transform.TransformDirection(zeroGInput);

        rb.AddForce(moveDirection * movementspeed * zeroGSpeedMultiplier, ForceMode.Force);
    }

    public void SetMovementState(MovementState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case MovementState.PlanetBound:
                if (planetGravity != null)
                    planetGravity.enabled = true;

                rb.linearDamping = 1f;
                break;

            case MovementState.ZeroG:
                if (planetGravity != null)
                    planetGravity.enabled = false;

                rb.linearDamping = 0.5f;
                break;
        }
    }

    public void ToggleState()
    {
        if (currentState == MovementState.PlanetBound)
            SetMovementState(MovementState.ZeroG);
        else
            SetMovementState(MovementState.PlanetBound);
    }

    private void OnCollisionStay()
    {
        grounded = true;
    }
}