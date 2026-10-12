using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle UI Refrence")]
    [SerializeField] PuzzleUI puzzleUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TogglePuzzleUI(bool toggle)
    {
        if (puzzleUI != null)
        {
            puzzleUI.TogglePuzzlePanel(toggle);
        }
    }
}
