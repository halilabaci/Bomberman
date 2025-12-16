using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadDesert()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_Desert");
    }

    public void LoadForest()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_Forest");
    }

    public void LoadCity()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_City");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
