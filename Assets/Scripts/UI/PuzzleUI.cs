using MemoryMatch;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    private GameObject PuzzlePanel;

    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);
    [SerializeField] private RectOffset padding = new RectOffset();

    private GridLayoutGroup layout;

    // Start is called before the first frame update
    void Start()
    {
        PuzzlePanel = gameObject;

        if (PuzzlePanel != null)
        {
            PuzzlePanel.SetActive(false);
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
        if (layout == null && !TryGetComponent(out layout))
            layout = gameObject.AddComponent<GridLayoutGroup>();

        var rect = (RectTransform)transform;

        // Largest square cell that fits every row and column inside this rect.
        float availableW = rect.rect.width - padding.horizontal - spacing.x * (columns - 1);
        float availableH = rect.rect.height - padding.vertical - spacing.y * (rows - 1);
        float cell = Mathf.Max(0f, Mathf.Min(availableW / columns, availableH / rows));

        layout.padding = padding;
        layout.spacing = spacing;
        layout.cellSize = new Vector2(cell, cell);
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = columns;
    }

}
