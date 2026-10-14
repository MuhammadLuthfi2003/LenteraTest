using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Player Refrences")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Detection")]
    [Tooltip("Sphere center, in the player's local space (x right, y up, z forward).")]
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 1f, 0.3f);
    [SerializeField] private float radius = 2f;

    [Tooltip("Max horizontal angle from the player's forward direction.")]
    [Range(1f, 180f)][SerializeField] private float maxAngle = 70f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Tooltip("Layers that block line of sight (walls, props). Do NOT include the player's layer.")]
    [SerializeField] private LayerMask obstructionMask = ~0;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private int maxColliders = 32;

    [Tooltip("Seconds between scans. 0 = every frame.")]
    [SerializeField] private float scanInterval = 0.05f;

    [Header("Scoring")]
    [Tooltip("How much each degree of angle hurts the score, compared to 1 meter of distance.")]
    [SerializeField] private float angleWeight = 0.02f;
    [Tooltip("Bonus for the current target so the prompt doesn't flicker between close objects.")]
    [SerializeField] private float switchMargin = 0.3f;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private bool drawOnlyWhenSelected = false;

    public event Action<IInteractable> TargetChanged;

    public IInteractable CurrentTarget { get; private set; }

    private Collider[] hits;
    private float scanTimer;
    private Transform currentTargetTransform;

    // Gizmo debug data (cleared and refilled on every scan)
    private enum CandidateState { OutOfAngle, Blocked, Cannot, Valid, Selected }

    private InputAction InteractAction;

    private struct DebugCandidate
    {
        public Vector3 point;
        public Vector3 losEnd;
        public CandidateState state;
    }

    private readonly List<DebugCandidate> debugCandidates = new List<DebugCandidate>();

    private Vector3 Origin => transform.TransformPoint(originOffset);

    private void Awake()
    {
        hits = new Collider[Mathf.Max(1, maxColliders)];
    }

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
                InteractAction = inputActions.FindAction("Interact");
                if (InteractAction != null)
                {
                    InteractAction.Enable();
                    InteractAction.performed += InteractPressed;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (InteractAction != null)
        {
            InteractAction.performed -= InteractPressed;
            InteractAction.Disable();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hits = new Collider[Mathf.Max(1, maxColliders)];
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            Scan();
        }
    }

    private void InteractPressed(InputAction.CallbackContext context)
    {
        // currentTargetTransform uses Unity's null check, so destroyed objects count as null
        if (CurrentTarget != null && currentTargetTransform != null)
        {
            CurrentTarget.Interact(this);
        }
    }

    private void Scan()
    {
        debugCandidates.Clear();

        Vector3 origin = Origin;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        int count = Physics.OverlapSphereNonAlloc(
            origin, radius, hits, interactMask, QueryTriggerInteraction.Collide);

        IInteractable best = null;
        Transform bestTransform = null;
        float bestScore = float.NegativeInfinity;
        int bestDebugIndex = -1;

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i];

            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            Transform interactableTransform = ((MonoBehaviour)interactable).transform;

            Vector3 point = col.ClosestPoint(origin);
            Vector3 toTarget = point - origin;
            float dist = toTarget.magnitude;

            // Horizontal angle only, so height differences don't affect facing.
            Vector3 flat = Vector3.ProjectOnPlane(toTarget, Vector3.up);
            float angle = flat.sqrMagnitude < 0.0001f ? 0f : Vector3.Angle(forward, flat);

            var dbg = new DebugCandidate { point = point, losEnd = point, state = CandidateState.Valid };

            if (angle > maxAngle)
            {
                dbg.state = CandidateState.OutOfAngle;
                debugCandidates.Add(dbg);
                continue;
            }

            if (requireLineOfSight)
            {
                Vector3 target = col.bounds.center;
                dbg.losEnd = target;

                if (Physics.Linecast(origin, target, out RaycastHit hit, obstructionMask,
                        QueryTriggerInteraction.Ignore)
                    && !hit.transform.IsChildOf(interactableTransform))
                {
                    dbg.losEnd = hit.point;
                    dbg.state = CandidateState.Blocked;
                    debugCandidates.Add(dbg);
                    continue;
                }
            }

            if (!interactable.CanInteract(this))
            {
                dbg.state = CandidateState.Cannot;
                debugCandidates.Add(dbg);
                continue;
            }

            float score = -dist - angle * angleWeight;

            if (interactableTransform == currentTargetTransform)
                score += switchMargin;

            debugCandidates.Add(dbg);

            if (score > bestScore)
            {
                bestScore = score;
                best = interactable;
                bestTransform = interactableTransform;
                bestDebugIndex = debugCandidates.Count - 1;
            }
        }

        if (bestDebugIndex >= 0)
        {
            DebugCandidate selected = debugCandidates[bestDebugIndex];
            selected.state = CandidateState.Selected;
            debugCandidates[bestDebugIndex] = selected;
        }

        SetTarget(best, bestTransform);
    }

    private void SetTarget(IInteractable target, Transform targetTransform)
    {
        // Destroyed objects look null to ==, so compare raw references instead
        if (ReferenceEquals(targetTransform, currentTargetTransform)) return;

        // Only notify the old target if it still exists
        if (currentTargetTransform != null)
            CurrentTarget?.OnLostFocus(this);

        currentTargetTransform = targetTransform;
        CurrentTarget = target;

        if (targetTransform != null)
            CurrentTarget?.OnFocus(this);

        TargetChanged?.Invoke(target);
    }

    // ------------------------------------------------------------- Toggle Controls

    public void ToggleInteraction(bool toggle)
    {
        if (InteractAction != null)
        {
            if (toggle)
            {
                InteractAction.Enable();
            }
            else
            {
                InteractAction.Disable();
            }
        }
    }


    // ---------------------------------------------------------------- Gizmos

    private void OnDrawGizmos()
    {
        if (!drawOnlyWhenSelected) DrawDebug();
    }

    private void OnDrawGizmosSelected()
    {
        if (drawOnlyWhenSelected) DrawDebug();
    }

    private void DrawDebug()
    {
        if (!drawGizmos) return;

        Vector3 origin = Origin;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        forward.Normalize();

        // Detection sphere
        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);
        Gizmos.DrawSphere(origin, radius);
        Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
        Gizmos.DrawWireSphere(origin, radius);

        // Facing cone (horizontal), with an arc at the sphere's edge
        Gizmos.color = Color.cyan;
        Vector3 left = Quaternion.AngleAxis(-maxAngle, Vector3.up) * forward * radius;
        Vector3 right = Quaternion.AngleAxis(maxAngle, Vector3.up) * forward * radius;
        Gizmos.DrawLine(origin, origin + left);
        Gizmos.DrawLine(origin, origin + right);

        const int segments = 24;
        Vector3 prev = origin + left;
        for (int i = 1; i <= segments; i++)
        {
            float a = Mathf.Lerp(-maxAngle, maxAngle, i / (float)segments);
            Vector3 next = origin + Quaternion.AngleAxis(a, Vector3.up) * forward * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        // Forward direction
        Gizmos.DrawLine(origin, origin + forward * radius);

        // Candidates from the last scan (runtime only)
        foreach (DebugCandidate c in debugCandidates)
        {
            switch (c.state)
            {
                case CandidateState.OutOfAngle:
                    Gizmos.color = Color.red;      // outside the facing cone
                    Gizmos.DrawWireSphere(c.point, 0.08f);
                    break;

                case CandidateState.Blocked:
                    Gizmos.color = Color.yellow;   // line of sight blocked
                    Gizmos.DrawLine(origin, c.losEnd);
                    Gizmos.DrawWireSphere(c.losEnd, 0.1f);
                    Gizmos.DrawWireSphere(c.point, 0.08f);
                    break;

                case CandidateState.Cannot:
                    Gizmos.color = Color.gray;     // visible but CanInteract() == false
                    Gizmos.DrawLine(origin, c.losEnd);
                    Gizmos.DrawWireSphere(c.point, 0.08f);
                    break;

                case CandidateState.Valid:
                    Gizmos.color = Color.green;    // valid, but not the best one
                    Gizmos.DrawLine(origin, c.losEnd);
                    Gizmos.DrawWireSphere(c.point, 0.1f);
                    break;

                case CandidateState.Selected:
                    Gizmos.color = Color.magenta;  // current target
                    Gizmos.DrawLine(origin, c.losEnd);
                    Gizmos.DrawSphere(c.point, 0.15f);
                    break;
            }
        }
    }

}
