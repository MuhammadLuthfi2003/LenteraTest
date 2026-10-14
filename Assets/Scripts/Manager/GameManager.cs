using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum GameStates
{
    Running,
    InPuzzle,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Gameobject")]
    [SerializeField] PlayerMovement player;

    [Header("Managers")]
    [SerializeField] public PuzzleManager puzzleManager;
    [SerializeField] public ScoreManager scoreManager;

    private GameStates state = GameStates.Running; 
    public GameStates State { get { return state; } }
    public PlayerMovement Player { get { return player; } }

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

    public void TogglePlayerControls(bool toggle)
    {
        PlayerLook playerLook = player.GetComponent<PlayerLook>();
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();

        playerLook.ToggleCameraControl(toggle);
        playerInteraction.ToggleInteraction(toggle);
        player.ToggleControls(toggle);
    }

    public void ToggleCursor(bool toggle)
    {
        if (toggle)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetState(GameStates state)
    {
        this.state = state;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
