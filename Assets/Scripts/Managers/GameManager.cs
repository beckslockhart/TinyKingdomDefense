using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject hudPanel;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    
    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        hudPanel.SetActive(true);
    }

    
    private void Update()
    {
        if (Keyboard.current == null || IsGameOver)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    
    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    
    public void PauseGame()
    {
        if (IsGameOver)
        {
            return;
        }

        IsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    
    public void ResumeGame()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    
    public void EndGame()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        IsPaused = false;

        pausePanel.SetActive(false);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}