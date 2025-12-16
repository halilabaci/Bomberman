using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    public GameObject levelSelectPanel;

    public void OpenLevelSelect()
    {
        Debug.Log("PLAY CLICKED");
        if (levelSelectPanel == null)
        {
            Debug.LogError("levelSelectPanel NULL!");
            return;
        }
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        Debug.Log("BACK CLICKED");
        if (levelSelectPanel) levelSelectPanel.SetActive(false);
    }
}
