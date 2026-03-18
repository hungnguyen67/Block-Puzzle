using UnityEngine;
using UnityEngine.UI; 
using TMPro;          
using UnityEngine.SceneManagement; 

public class GameOverManager : MonoBehaviour
{
  
    public static GameOverManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel; 

    [Header("UI Elements (Optional)")]
    [SerializeField] private TextMeshProUGUI finalScoreText; 

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("GameOverManager: Chưa gán GameOverPanel trong Inspector!");
        }
    }


    public void ShowGameOver(int score = 0)
    {
        if (gameOverPanel == null) return;

        gameOverPanel.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text = "ĐIỂM: " + score.ToString();
        }

    }

    public void RestartGame()
    {
        Debug.Log("Đang tải lại game...");
      
        Time.timeScale = 1f; 

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}