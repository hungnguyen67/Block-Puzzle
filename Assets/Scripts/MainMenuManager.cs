using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Gameplay"); 
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}