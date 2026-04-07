using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private UnityEngine.UI.Image musicIcon;
    [SerializeField] private UnityEngine.UI.Image soundIcon;

    [Header("Reward Flow")]
    [SerializeField] private RewardFlow rewardFlow;
    private bool hasRevived = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Ensure panels are closed at the start
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        hasRevived = false;
    }

    #region Settings
    public void OpenSettings()
    {
        // MỚI: Chỉ cần chơi click, nút bấm sẽ tự động nhận diện rồi
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f; 
        }
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }
    }
    #endregion

    #region Navigation
    public void RestartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        Time.timeScale = 1f;
        hasRevived = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToHome()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    #endregion

    #region Game Over
    public void ShowGameOver(int score)
    {
        // If not revived yet, show reward flow first
        if (!hasRevived && rewardFlow != null)
        {
            hasRevived = true; // Mark as revived once
            rewardFlow.ShowContinueFlow(score);
            Time.timeScale = 0f; 
            return;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Cập nhật điểm cao nhất ngay tại đây
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt("BestScore", bestScore);
                PlayerPrefs.Save();
            }
            
            if (finalScoreText != null)
            {
                finalScoreText.text = "Score " + score.ToString();
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = "BestScore " + bestScore.ToString();
            }

            Time.timeScale = 0f; // Pause the game on game over
        }
    }
    #endregion
}
