using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginUI : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    private UserRepository userRepo = new UserRepository();

    public void OnRegister()
    {
        var user = userRepo.Register(usernameInput.text, passwordInput.text);
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
        var user = userRepo.Login(usernameInput.text, passwordInput.text);
        if (user == null)
        {
            statusText.color = Color.red;
            statusText.text = "Login failed (wrong username/password)";
            return;
        }

        SceneManager.LoadScene("MainMenu"); 
    }
}
