using MemoryMatch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private GameObject PuzzlePanel;
    [SerializeField] public GameObject gridPanel;

    [Tooltip("Resize the grid panel to Panel Size. If off, the panel keeps the size set in the scene.")]
    [SerializeField] private bool useCustomPanelSize = false;
    [SerializeField] private Vector2 panelSize = new Vector2(600f, 600f);

    [Header("Grid Settings")]
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);
    [SerializeField] private RectOffset padding = new RectOffset();

    [Tooltip("On: cells are the largest squares that fit the panel. Off: uses Cell Size below.")]
    [SerializeField] private bool autoCellSize = true;
    [SerializeField] private Vector2 cellSize = new Vector2(100f, 100f);

    private GridLayoutGroup layout;

    private PuzzleManager puzzleManager;
    private GameManager gameManager;

    private void OnEnable()
    {
        PuzzleManager.OnPuzzleSolved += FinishGame;
    }

    private void OnDisable()
    {
        PuzzleManager.OnPuzzleSolved -= FinishGame;
    }

    // Start is called before the first frame update
    void Start()
    {
        PuzzlePanel = gameObject;

        if (PuzzlePanel != null)
        {
            PuzzlePanel.SetActive(false);
        }

        if (GameManager.Instance != null && gameManager == null)
        {
            gameManager = GameManager.Instance;
            puzzleManager = gameManager.puzzleManager;
        }
    }

    // Used to Toggle Panel
    public void TogglePuzzlePanel(bool toggle)
    {
        if (PuzzlePanel != null)
        {
            PuzzlePanel.SetActive(toggle);
        }
    }


    public void ConfigureLayout(int columns, int rows)
    {
        if (gridPanel == null)
        {
            Debug.LogError("[PuzzleUI] Assign the grid panel.", this);
            return;
        }

        if (layout == null && !gridPanel.TryGetComponent(out layout))
            layout = gridPanel.AddComponent<GridLayoutGroup>();

        var rect = (RectTransform)gridPanel.transform;

        // Optionally resize the panel (works with any anchor setup).
        if (useCustomPanelSize)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelSize.x);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelSize.y);
        }

        float panelW = useCustomPanelSize ? panelSize.x : rect.rect.width;
        float panelH = useCustomPanelSize ? panelSize.y : rect.rect.height;

        Vector2 cell;
        if (autoCellSize)
        {
            // Largest square cell that fits every row and column inside the panel.
            float availableW = panelW - padding.horizontal - spacing.x * (columns - 1);
            float availableH = panelH - padding.vertical - spacing.y * (rows - 1);
            float size = Mathf.Max(0f, Mathf.Min(availableW / columns, availableH / rows));
            cell = new Vector2(size, size);
        }
        else
        {
            cell = new Vector2(Mathf.Max(1f, cellSize.x), Mathf.Max(1f, cellSize.y));

            float neededW = cell.x * columns + spacing.x * (columns - 1) + padding.horizontal;
            float neededH = cell.y * rows + spacing.y * (rows - 1) + padding.vertical;
            if (neededW > panelW || neededH > panelH)
                Debug.LogWarning($"[PuzzleUI] Grid needs {neededW}x{neededH} but the panel is {panelW}x{panelH}. Cards will overflow.", this);
        }

        layout.padding = padding;
        layout.spacing = spacing;
        layout.cellSize = cell;
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;
    }

    public void FinishGame()
    {
        if (gameManager && puzzleManager)
        {
            gameManager.ToggleCursor(true);
            gameManager.TogglePlayerControls(true);
            puzzleManager.StopPuzzle();
            TogglePuzzlePanel(false);
        }
    }

}