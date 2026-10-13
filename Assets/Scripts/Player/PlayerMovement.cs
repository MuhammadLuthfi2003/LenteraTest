using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

enum SprintState
{
    NotSprinting,
    Sprinting,
    Cooldown
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Sprinting")]
    [SerializeField] private float sprintDuration = 5f;
    [SerializeField] private float sprintCooldown = 3f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded = true;

    private float gravity = Physics.gravity.y;
    private SprintState sprintState = SprintState.NotSprinting;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;

    Vector2 moveInput;
    Vector3 moveDirection;

    public event Action<float> OnMoveAction;
    public event Action OnJumped;

    private void OnEnable()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }

        if (playerInput != null)
        {
            InputActionMap inputActions = playerInput.actions.FindActionMap("Player");

            if (inputActions != null)
            {
                moveAction = inputActions.FindAction("Move");
                jumpAction = inputActions.FindAction("Jump");
                sprintAction = inputActions.FindAction("Sprint");
            }

            if (moveAction != null)
            {
                moveAction.Enable();
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            if (jumpAction != null)
            {
                jumpAction.Enable();
                jumpAction.performed += OnJump;
                jumpAction.canceled += OnJump;
            }

            if (sprintAction != null)
            {
                sprintAction.Enable();
                sprintAction.performed += OnSprint;
                sprintAction.canceled += OnSprint;
            }
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Disable();
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }
        if (jumpAction != null)
        {
            jumpAction.Disable();
            jumpAction.performed -= OnJump;
            jumpAction.canceled -= OnJump;
        }
        if (sprintAction != null)
        {
            sprintAction.Disable();
            sprintAction.performed -= OnSprint;
            sprintAction.canceled -= OnSprint;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnJumped?.Invoke();
        }
    }

    void OnSprint(InputAction.CallbackContext context)
    {
        if (isGrounded && sprintState.Equals(SprintState.NotSprinting))
        {
            StartCoroutine(SprintCycle());
        }
    }

    IEnumerator SprintCycle()
    {
        sprintState = SprintState.Sprinting;
        yield return new WaitForSeconds(sprintDuration);
        sprintState = SprintState.Cooldown;
        yield return new WaitForSeconds(sprintCooldown);
        sprintState = SprintState.NotSprinting;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!characterController) return;

        // Ground Check
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            // Reset velocity downward force when grounded to prevent buildup
            velocity.y = -2f;
        }

        // Camera-relative direction (flattened so camera pitch doesn't slow down)
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        // Face the movement direction 
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        // Speed check
        float currentSpeed = sprintState.Equals(SprintState.Sprinting) ? sprintSpeed : walkSpeed;

        if (moveInput.sqrMagnitude <= 0)
        {
            // sends signal to animator
            OnMoveAction?.Invoke(0);
        }
        else
        {
            // sends signal to animator
            OnMoveAction?.Invoke(currentSpeed);
        }

        characterController.Move(moveDirection * currentSpeed * Time.fixedDeltaTime);
        
        // gravity application
        velocity.y += gravity * Time.deltaTime;

        // Move the character vertically (Gravity / Jumping)
        characterController.Move(velocity * Time.deltaTime);
    }

    public void ToggleControls(bool toggle)
    {
        if (toggle)
        {
            if (moveAction != null)
            {
                moveAction.Enable();
            }

            if (jumpAction != null)
            {
                jumpAction.Enable();
            }

            if (sprintAction != null)
            {
                sprintAction.Enable();
            }
        }
        else
        {
            if (moveAction != null)
            {
                moveAction.Disable();
            }

            if (jumpAction != null)
            {
                jumpAction.Disable();
            }

            if (sprintAction != null)
            {
                sprintAction.Disable();
            }
        }
    }
}
