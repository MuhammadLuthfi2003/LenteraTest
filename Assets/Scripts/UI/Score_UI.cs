using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score_UI : MonoBehaviour
{
    [Header("Refrence")]
    [SerializeField] TextMeshProUGUI scoreDisplay;

    public void UpdateScore(int score)
    {
        if (scoreDisplay)
        {
            scoreDisplay.text = "Score : " + score.ToString();
        }
    }
}
