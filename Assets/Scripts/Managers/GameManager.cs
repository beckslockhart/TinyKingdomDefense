using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject hudPanel;

    public bool IsGameOver { get; private set; }

    
    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }
    }

    
    public void EndGame()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        Time.timeScale = 0f;
    }

    
    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}