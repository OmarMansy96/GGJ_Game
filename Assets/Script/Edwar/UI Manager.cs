using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject pauseMenu;

    void Start()
    {
        ShowStartMenu();
    }
    public void ShowStartMenu()
    {
        startMenu.SetActive(true);
        gameUI.SetActive(false);
        loseMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f; 
    }


    public void StartGame()
    {
        startMenu.SetActive(false);
        gameUI.SetActive(true);
        Time.timeScale = 1f; 
    }
    public void ShowLoseMenu()
    {
        loseMenu.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
    }
    public void ShowPauseMenu() { 
        
        pauseMenu.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;

    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
