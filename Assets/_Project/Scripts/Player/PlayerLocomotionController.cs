using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerLocomotionController : MonoBehaviour {

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera mainCam;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float combatMoveSpeed = 2f;

    [Header("Acceleration")]
    [SerializeField] private float accelerationTime = 0.2f;
    [SerializeField] private float decelerationTime = 0.15f;
    private Vector3 attackMovement;
    [Header("Animator Damping")]
    [SerializeField] private float speedDampTime = 0.1f;
    [SerializeField] private float velocityDampTime = 0.1f;

    // Input
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isSprinting;
    private bool isWalking;

    // Movement
    private Vector3 moveDirection;
    private float currentSpeed;
    private float targetSpeed;
    private float velocityX;
    private float velocityZ;

    // State
    private bool isInCombat;
    private float movementMultiplier = 1f;

    // Animation hashes
    private int speedHash;

    // Public properties
    public Vector2 MoveInput => moveInput;
    public Vector3 MoveDirection => moveDirection;
    public bool IsMoving => moveInput.magnitude > 0.1f;
    public bool IsSprinting => isSprinting;
    public bool IsWalking => isWalking;

    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        if (controller == null) controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();
        mainCam = Camera.main;

        inputActions = new InputSystem_Actions();
        speedHash = Animator.StringToHash("Speed");
    }

    private void OnEnable() {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Walk.performed += OnWalkPerformed;
    }

    private void OnDisable() {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Sprint.performed -= OnSprintPerformed;
        inputActions.Player.Walk.performed -= OnWalkPerformed;
        inputActions.Player.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context) {
        moveInput = Vector2.zero;
    }

    private void OnSprintPerformed(InputAction.CallbackContext context) {
        isSprinting = !isSprinting;
    }

    private void OnWalkPerformed(InputAction.CallbackContext context) {
        isWalking = !isWalking;
    }

    private void Update() {
        HandleMovement();
        HandleRotation();
        HandleAnimations();
    }

    private void HandleMovement() {

        if (attackMovement.magnitude > 0.1f) {
            controller.Move(attackMovement * Time.deltaTime);
            attackMovement = Vector3.zero; // reset every frame
            return;
        }

        // Determine target speed based on input and combat state
        if (moveInput.magnitude > 0.1f) {
            float baseSpeed;

            // Sprint only works when moving forward and not in combat
            if (isSprinting && !isInCombat) {
                baseSpeed = sprintSpeed;
            }
            else if (isWalking && !isInCombat) {
                baseSpeed = walkSpeed;
            }
            else {
                baseSpeed = isInCombat ? combatMoveSpeed : runSpeed;
            }

            // Apply movement multiplier (set by combat system)
            targetSpeed = baseSpeed * movementMultiplier;
        }
        else {
            targetSpeed = 0f;
        }

        // Smoothly accelerate/decelerate to target speed
        float acceleration = targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / acceleration);

        // Calculate camera-relative movement direction
        if (moveInput.magnitude > 0.1f) {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

            // Apply movement
            controller.Move(currentSpeed * Time.deltaTime * moveDirection);
        }
        else {
            moveDirection = Vector3.zero;
        }

        // Apply gravity
        controller.Move(9.81f * Time.deltaTime * Vector3.down);
    }

    private void HandleRotation() {
        Vector3 lookDirection = Vector3.zero;

        if (isInCombat) {
            // In combat, face camera direction
            // lookDirection = Camera.main.transform.forward;
        }
        else if (moveDirection.magnitude > 0.1f) {
            // Outside combat, face movement direction
            lookDirection = moveDirection;
        }
        else {
            return; // Don't rotate if not moving
        }

        lookDirection.y = 0f;

        if (lookDirection.magnitude > 0.1f) {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleAnimations() {
        // Calculate velocity relative to character's forward direction
        Vector3 localVelocity = transform.InverseTransformDirection(moveDirection * currentSpeed);

        // Smooth the velocity values for blend tree
        velocityX = Mathf.Lerp(velocityX, localVelocity.x, Time.deltaTime / velocityDampTime);
        velocityZ = Mathf.Lerp(velocityZ, localVelocity.z, Time.deltaTime / velocityDampTime);

        // Normalize speed (0 to 1 range where 1 = sprint speed)
        float normalizedSpeed = currentSpeed / sprintSpeed;

        // Update animator parameters
        animator.SetFloat(speedHash, normalizedSpeed, speedDampTime, Time.deltaTime);
    }

    // Public methods for combat system

    public void ApplyAttackMovement(Vector3 worldDelta, float deltaYaw) {
        if (Mathf.Abs(deltaYaw) > 0.001f) {
            transform.Rotate(0f, deltaYaw, 0f);
        }
        controller.Move(worldDelta);
    }

    public void ApplyAttackMovement(Vector3 worldDelta) {
        
        controller.Move(worldDelta);
    }
    public void SetCombatMode(bool inCombat) {
        isInCombat = inCombat;
        if (inCombat && isSprinting) {
            isSprinting = false; // Can't sprint in combat
        }
    }

    public void SetMovementMultiplier(float multiplier) {
        movementMultiplier = Mathf.Clamp01(multiplier);
    }

    public void LockMovement() {
        movementMultiplier = 0f;
    }

    public (Vector3 _worldMoveDir, Vector3 _localMoveDir) GetDodgeDirection() {

        //ghost of tsushima type dodging i need
        Vector3 localMoveDir = Vector3.zero;

        if (moveDirection.magnitude > 0.1f) {
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 worldMoveDir = camForward * moveInput.y + camRight * moveInput.x;
            worldMoveDir.Normalize();
            localMoveDir = transform.InverseTransformDirection(worldMoveDir);

            return (worldMoveDir, localMoveDir);
        }
        else {
            return (Vector3.zero, Vector3.zero);
        }
    }

    public void UnlockMovement() {
        movementMultiplier = 1f;
    }


}


public enum DODGE_DIRECTION {
    FORWARD, BACK, LEFT, RIGHT
}