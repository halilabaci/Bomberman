using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    // ✅ Tek giriş noktası (Facade)
    private GameDataService dataService;

    private void Awake()
    {
        dataService = new GameDataService();
    }

    public void OnRegister()
    {
        var username = usernameInput.text;
        var password = passwordInput.text;

        var user = dataService.Register(username, password);
        if (user == null)
        {
            statusText.color = Color.red;
            statusText.text = "Register failed (user exists or empty)";
            return;
        }

        statusText.color = Color.white;
        statusText.text = "Registered! You can login now.";
    }

    public void OnLogin()
    {
        var username = usernameInput.text;
        var password = passwordInput.text;

        var user = dataService.Login(username, password);
        if (user == null)
        {
            statusText.color = Color.red;
            statusText.text = "Login failed (wrong username/password)";
            return;
        }

        // ✅ Session tek noktadan set edilir
        Session.SetUser(user.Id, user.Username);

        // ✅ Theme DB'den servis üzerinden okunur
        var theme = dataService.GetTheme(user.Id);
        Debug.Log("[Theme] " + theme);

        SceneManager.LoadScene("MainMenu");
    }
}
