using UnityEngine;
using UnityEngine.SceneManagement;
using DPBomberman.Models; // ✅ ThemeType burada

public class SceneController : MonoBehaviour
{
    public void LoadDesert()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame(ThemeType.Desert);
        else
            SceneManager.LoadScene("Level_Desert");
    }

    public void LoadForest()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame(ThemeType.Forest);
        else
            SceneManager.LoadScene("Level_Forest");
    }

    public void LoadCity()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame(ThemeType.City);
        else
            SceneManager.LoadScene("Level_City");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame() => Application.Quit();
}
