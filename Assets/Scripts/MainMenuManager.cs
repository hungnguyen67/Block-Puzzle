using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    // Hàm này sẽ gắn vào nút Play
    public void StartGame()
    {
        Debug.Log("Đang vào game...");
       
        SceneManager.LoadScene("Gameplay"); 
    }

    // Hàm này sẽ gắn vào nút Bảng Xếp Hạng (Cúp)
    public void ShowLeaderboard()
    {
        Debug.Log("Đang mở Bảng xếp hạng...");
        SceneManager.LoadScene("Leaderboard");
     
    }
}