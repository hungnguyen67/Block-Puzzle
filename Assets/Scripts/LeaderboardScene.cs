using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LeaderboardScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI[] topScoreTexts; 

    void Start()
    {
        for (int i = 0; i < topScoreTexts.Length; i++)
        {
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            topScoreTexts[i].text = "TOP " + (i + 1) + ": " + score.ToString();
        }
    }

    public void BackToMainMenu()
    {

        SceneManager.LoadScene("MainMenu");
    }
}