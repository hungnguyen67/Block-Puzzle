using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LeaderboardScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI[] topScoreTexts; // Kéo 5 dòng Text Top 1 -> Top 5 vào ô này trong Inspector

    void Start()
    {
        // Vừa mở màn hình lên là lấy điểm từ máy in ra luôn
        for (int i = 0; i < topScoreTexts.Length; i++)
        {
            // Lấy điểm từ bộ nhớ, nếu chưa có ai chơi thì để mặc định là 0
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            topScoreTexts[i].text = "TOP " + (i + 1) + ": " + score.ToString();
        }
    }

    // Hàm này gắn vào Nút Home
    public void BackToMainMenu()
    {
        Debug.Log("Quay lại Menu Chính...");
        SceneManager.LoadScene("MainMenu");
    }
}