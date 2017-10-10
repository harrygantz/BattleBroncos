using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour {

    public void Play() {
        SceneManager.LoadScene("Scene1");
    }

    public void Quit() {
        Application.Quit();
    }
}
