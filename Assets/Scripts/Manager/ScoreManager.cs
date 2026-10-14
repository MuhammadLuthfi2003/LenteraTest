using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Refrence")]
    [SerializeField] Score_UI scoreUI;
    [SerializeField] Win_UI winUI;

    [Header("Current Score")]
    [SerializeField] int currentScore = 0;

    [Header("Game Completion")]
    [SerializeField] int currentFinishes = 0;
    [SerializeField] int maxFinishes = 6;

    // Start is called before the first frame update
    void Start()
    {
        if (scoreUI)
        {
            scoreUI.UpdateScore(currentScore);
            scoreUI.UpdateFinishes(currentFinishes, maxFinishes);
        }

        PuzzleManager.OnAddScore += AddScore;
        PuzzleManager.OnPuzzleSolved += IncrementFinishes;
    }

    private void OnDestroy()
    {
        PuzzleManager.OnAddScore -= AddScore;
        PuzzleManager.OnPuzzleSolved -= IncrementFinishes;
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;

        if (scoreUI)
        {
            scoreUI.UpdateScore(currentScore);
        }

    }

    public void IncrementFinishes()
    {
        currentFinishes++;
        scoreUI.UpdateFinishes(currentFinishes, maxFinishes);

        if (currentFinishes >= maxFinishes)
        {
            if (!winUI) return;
            winUI.GameFinished(currentScore);
        }
    }
}
