using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Win_UI : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] TextMeshProUGUI finalScoreText;

    private GameObject winUIPanel;

    // Start is called before the first frame update
    void Start()
    {
        winUIPanel = gameObject;
        ToggleWinScreen(false);
    }

    public void ToggleWinScreen(bool toggle)
    {
        if (winUIPanel)
        {
            winUIPanel.SetActive(toggle);
        }
    }

    public void GameFinished(int finalScore = 0)
    {
        winUIPanel.SetActive(true);


        StartCoroutine(DelayFinished(2, finalScore));
    }

    IEnumerator DelayFinished(float delay, int finalScore)
    {
        yield return new WaitForSeconds(delay);
        if (finalScoreText)
        {
            finalScoreText.text = "Final Score : " + finalScore.ToString();
        }

        if (GameManager.Instance)
        {
            GameManager.Instance.ToggleCursor(false);
        }

        Time.timeScale = 0;
    }
}
