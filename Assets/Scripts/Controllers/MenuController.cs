using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void PlayForest() => SceneManager.LoadScene("Level_Forest");
    public void PlayDesert() => SceneManager.LoadScene("Level_Desert");
    public void PlayCity() => SceneManager.LoadScene("Level_City");
    public void Quit() => Application.Quit();
}
