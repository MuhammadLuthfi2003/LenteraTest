using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Score UI Refrence")]
    [SerializeField] Score_UI scoreUI;

    [Header("Current Score")]
    [SerializeField] int currentScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (scoreUI)
        {
            scoreUI.UpdateScore(currentScore);
        }

        PuzzleManager.OnAddScore += AddScore;
    }

    private void OnDestroy()
    {
        PuzzleManager.OnAddScore -= AddScore;
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;

        if (scoreUI)
        {
            scoreUI.UpdateScore(currentScore);
        }

    }
}
