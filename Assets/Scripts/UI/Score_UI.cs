using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Score_UI : MonoBehaviour
{
    [Header("Refrence")]
    [SerializeField] TextMeshProUGUI scoreDisplay;
    [SerializeField] TextMeshProUGUI finishesDisplay;

    public void UpdateScore(int score)
    {
        if (scoreDisplay)
        {
            scoreDisplay.text = "Score : " + score.ToString();
        }
    }

    public void UpdateFinishes(int finishes, int maxFinishes)
    {
        if (finishesDisplay)
        {
            finishesDisplay.text = "Puzzles Finished : " + finishes.ToString() + "/" + maxFinishes.ToString();
        }
    }
}
