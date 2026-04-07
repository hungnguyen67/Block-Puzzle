using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText; 
    public GameObject floatingScorePrefab;

    private int currentScore = 0;

    public void AddScore(int amount, bool spawnText = true)
    {
        AddScore(amount, Camera.main != null ? Camera.main.transform.position + Vector3.forward * 10 : Vector3.zero, spawnText);
    }

    public void AddScore(int amount, Vector3 pos, bool spawnText = true)
    {
        currentScore += amount;
        UpdateScoreUI();
        if (spawnText)
        {
            SpawnFloatingScore(amount, pos);
        }
    }

    private void SpawnFloatingScore(int amount, Vector3 pos)
    {
        if (floatingScorePrefab == null) return;

        GameObject go = Instantiate(floatingScorePrefab, pos, Quaternion.identity);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        if (fs != null)
        {
            fs.Setup(amount);
        }
    }

    private void Awake()
    {
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


    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    public int GetCurrentScore() => currentScore;
}