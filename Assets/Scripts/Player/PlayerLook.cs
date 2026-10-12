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
    public event Action<float> OnScrollInput;

    InputAction LookAction;
    InputAction ScrollAction;

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
                ScrollAction = inputActions.FindAction("Scroll");

                if (LookAction != null)
                {
                    LookAction.Enable();
                    LookAction.performed += OnLookPerformed;
                    LookAction.canceled += OnLookPerformed;
                }

                if (ScrollAction != null)
                {
                    ScrollAction.Enable();
                    ScrollAction.performed += OnScrollPerformed;
                    ScrollAction.canceled += OnScrollPerformed;
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

        if (ScrollAction != null)
        {
            ScrollAction.Disable();
            ScrollAction.performed -= OnScrollPerformed;
            ScrollAction.canceled -= OnScrollPerformed;
        }
    }

    void OnLookPerformed(InputAction.CallbackContext context)
    {
        OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }

    void OnScrollPerformed(InputAction.CallbackContext context)
    {
        OnScrollInput?.Invoke(context.ReadValue<float>());
    }

    public void ToggleCameraControl(bool toggle)
    {
        if (toggle)
        {
            if (LookAction != null)
            {
                LookAction.Enable();
            }

            if (ScrollAction != null)
            {
                ScrollAction.Enable();
            }
        }
        else
        {
            if (LookAction != null)
            {
                LookAction.Disable();
            }

            if (ScrollAction != null)
            {
                ScrollAction.Disable();
            }
        }
    }
}
