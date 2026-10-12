using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleUI : MonoBehaviour
{
    private GameObject PuzzlePanel;

    // Start is called before the first frame update
    void Start()
    {
        PuzzlePanel = gameObject;

        if (PuzzlePanel != null)
        {
            PuzzlePanel.SetActive(false);
        }
    }

    // Update is called once per frame
    public void TogglePuzzlePanel(bool toggle)
    {
        if (PuzzlePanel != null)
        {
            PuzzlePanel.SetActive(toggle);
        }
    }
}
