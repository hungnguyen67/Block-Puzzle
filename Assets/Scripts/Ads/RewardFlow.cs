using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RewardFlow : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject continueMenu;
    [SerializeField] private GameObject gameOverMenu;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Button reviveButton;

    private Coroutine countdownCoroutine;
    private int currentScore;

    private void Start()
    {
        // Ẩn bảng khi bắt đầu game
        if (continueMenu != null) continueMenu.SetActive(false);

        if (reviveButton != null)
        {
            reviveButton.onClick.AddListener(OnReviveClicked);
        }
    }

    public void ShowContinueFlow(int score)
    {
        currentScore = score;
        if (continueMenu != null)
        {
            continueMenu.SetActive(true);
            if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
            countdownCoroutine = StartCoroutine(StartCountdown());
        }
        else
        {
            // Fallback if no continue menu
            ShowGameOver();
        }
    }

    private IEnumerator StartCountdown()
    {
        float timer = 10.5f; // Slightly more than 10 to show 10 clearly
        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.FloorToInt(timer).ToString();
            }
            timer -= Time.unscaledDeltaTime; // Use unscaled because game is likely paused
            yield return null;
        }

        // Timer ended, show Game Over
        OnTimerEnded();
    }

    private void OnTimerEnded()
    {
        if (continueMenu != null) continueMenu.SetActive(false);
        ShowGameOver();
    }

    private void OnReviveClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
        
        if (SingleAdController.Instance != null)
        {
            SingleAdController.Instance.ShowAdWithCallback(OnUserRewarded);
        }
        else
        {
            // For testing: if no ad, just reward
            OnUserRewarded();
        }
    }

    private void OnUserRewarded()
    {
        if (continueMenu != null) continueMenu.SetActive(false);
        
        // Resume game
        Time.timeScale = 1f;

        // Clear 2 middle rows
        Board board = Object.FindFirstObjectByType<Board>();
        if (board != null)
        {
            board.ClearMiddleRows();
            // Đợi 0.4s để hiệu ứng xóa hàng hoàn tất rồi mới kiểm tra độ mờ
            StartCoroutine(DelayRefresh(0.4f));
        }
    }

    private IEnumerator DelayRefresh(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Blocks blocks = Object.FindFirstObjectByType<Blocks>();
        if (blocks != null) blocks.RefreshAllBlocksTransparency();
    }

    public void ShowGameOver()
    {
        if (continueMenu != null) continueMenu.SetActive(false);
        
        if (GameplayUIManager.Instance != null)
        {
            GameplayUIManager.Instance.ShowGameOver(currentScore);
        }
        else if (gameOverMenu != null)
        {
            gameOverMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
