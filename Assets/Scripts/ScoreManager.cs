using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText; 

    private int currentScore = 0;

    private void Awake()
    {
        // Kiểm tra tránh trùng lặp Singleton
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // Hiển thị "DIEM: 000100" chẳng hạn
            scoreText.text = "DIEM: " + currentScore.ToString();
        }
    }

    public int GetCurrentScore() => currentScore;
}