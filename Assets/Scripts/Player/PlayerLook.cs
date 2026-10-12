using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    public event Action<Vector2> OnLookInput;

    InputAction LookAction;

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
                LookAction = inputActions.FindAction("Look");
                if (LookAction != null)
                {
                    LookAction.Enable();
                    LookAction.performed += OnLookPerformed;
                    LookAction.canceled += OnLookPerformed;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (LookAction != null)
        {
            LookAction.Disable();
            LookAction.performed -= OnLookPerformed;
            LookAction.canceled -= OnLookPerformed;
        }
    }

    void OnLookPerformed(InputAction.CallbackContext context)
    {
        OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }
}
