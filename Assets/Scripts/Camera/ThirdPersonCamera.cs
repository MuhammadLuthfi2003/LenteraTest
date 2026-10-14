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

    [Header("Collision Settings")]
    [SerializeField] private LayerMask collisionMask = ~0;      // set to Ground/Environment layers in Inspector
    [SerializeField] private float collisionRadius = 0.25f;      // camera "thickness"
    [SerializeField] private float collisionBuffer = 0.1f;       // small gap from the surface
    [SerializeField] private float pivotHeight = 1.5f;           // look at chest/head instead of feet
    [SerializeField] private float collisionSmoothTime = 0.08f;  // how fast the camera moves in/out

    [Header("Smoothness")]
    [SerializeField] private float smoothTime = 0.12f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private PlayerLook playerLook;

    private float currentDistance;
    private float distanceSmoothVelocity;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        currentDistance = distance;

        if (target.TryGetComponent(out playerLook))
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

        // Pivot point the camera orbits around
        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Vector3 direction = -transform.forward;

        // Check for obstacles between pivot and desired camera position
        float desiredDistance = distance;
        if (Physics.SphereCast(pivot, collisionRadius, direction, out RaycastHit hit,
                               distance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            desiredDistance = Mathf.Max(hit.distance - collisionBuffer, 0.1f);
        }

        // Snap in quickly when blocked, ease back out when clear
        float smooth = desiredDistance < currentDistance ? 0f : collisionSmoothTime;
        currentDistance = smooth == 0f
            ? desiredDistance
            : Mathf.SmoothDamp(currentDistance, desiredDistance, ref distanceSmoothVelocity, smooth);

        transform.position = pivot + direction * currentDistance;
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
