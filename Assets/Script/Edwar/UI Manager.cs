using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject loseMenu;
    [SerializeField] private GameObject pauseMenu;

    public float fadeInTime;
    public string GameName;
    public GameObject fadeIn;
    void Start()
    {
        // ShowStartMenu();
    }
    public void ShowStartMenu()
    {
        fadeIn.GetComponent<Animator>().Play("New Animation");
        Invoke("SceneTrans", fadeInTime);
    }
    public void SceneTrans()
    {
        SceneManager.LoadScene(GameName);
    }
    public void StartGame()
    {
        startMenu.SetActive(true);
        gameUI.SetActive(false);
        loseMenu.SetActive(false);
        pauseMenu.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ShowLoseMenu()
    {
        loseMenu.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
    }
    public void ShowPauseMenu()
    {

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
