using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Follow & Orbit Settings")]
    [SerializeField] private Transform target; 
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Vector2 verticalLookLimits = new Vector2(-20f, 60f);

    [Header("Camera Settings")]
    [SerializeField] private float mouseScrollStep = 0.001f;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 10.0f;

    [Header("Smoothness")]
    [SerializeField] private float smoothTime = 0.12f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private PlayerLook playerLook;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (target.TryGetComponent(out PlayerLook playerLook))
        {
            this.playerLook = playerLook;
        }

        // bind event from playerlook
        if (playerLook != null)
        {
            playerLook.OnLookInput += ReadCursorValues;
            playerLook.OnScrollInput += ReadScrollValues;
        }
    }

    private void OnDestroy()
    {
        if (playerLook != null)
        {
            playerLook.OnLookInput -= ReadCursorValues;
            playerLook.OnScrollInput -= ReadScrollValues;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // smooth out camera orbital rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(rotationY, rotationX), ref rotationSmoothVelocity, smoothTime);
        transform.localEulerAngles = currentRotation;

        // Position the camera behind the player target at the specified distance
        transform.position = target.position - transform.forward * distance;
    }

    void ReadCursorValues(Vector2 inputValue)
    {
        // get mouse input
        rotationX += inputValue.x * mouseSensitivity * Time.deltaTime;
        rotationY -= inputValue.y * mouseSensitivity * Time.deltaTime;

        // clamp
        rotationY = Mathf.Clamp(rotationY, verticalLookLimits.x, verticalLookLimits.y);
    }

    void ReadScrollValues(float scroll)
    {
        scroll = Mathf.Sign(scroll) * Mathf.Pow(Mathf.Abs(scroll), 1.5f); // Apply a non-linear curve to the scroll input
        distance -= scroll * mouseScrollStep; // Adjust the multiplier for sensitivity
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

}
