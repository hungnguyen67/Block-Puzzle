using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {

       
        SceneManager.LoadScene("Gameplay"); 
    }

    public void ShowLeaderboard()
    {

        SceneManager.LoadScene("Leaderboard");
     
    }
}