using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver: MonoBehaviour
{

    public GameObject GameOverScreen;

    private bool gameOver = false;

    void Start()
    {
        GameOverScreen.SetActive(false);
    }

    public void SetGameOver()
    {
        GameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
