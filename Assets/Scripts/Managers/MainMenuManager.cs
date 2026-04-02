using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay"); 
    }
}