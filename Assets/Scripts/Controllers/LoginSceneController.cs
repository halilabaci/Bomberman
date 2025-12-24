using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneController : MonoBehaviour
{
    private InputField username;
    private InputField password;
    private Text status;

    private UserRepository userRepo = new UserRepository();

    private void Start()
    {
        // DB hazýr deðilse init et (DbBoot yoksa bile çalýþsýn)
        DbManager.Init();

        BuildUI();
    }

    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem yoksa ekle (buton/input çalýþsýn)
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Panel
        var panel = UI.Box(canvasGO.transform, "Panel", new Vector2(520, 360), new Vector2(0.5f, 0.5f));

        UI.Label(panel, "Title", "Login / Register", 24, new Vector2(0, 130));

        username = UI.Input(panel, "Username", "Username", new Vector2(0, 60));
        password = UI.Input(panel, "Password", "Password", new Vector2(0, 10));
        password.contentType = InputField.ContentType.Password;

        UI.Button(panel, "LoginBtn", "Login", new Vector2(-110, -70), () => DoLogin());
        UI.Button(panel, "RegisterBtn", "Register", new Vector2(110, -70), () => DoRegister());

        status = UI.Label(panel, "Status", "", 14, new Vector2(0, -130));
    }

    private void DoRegister()
    {
        var u = userRepo.Register(username.text, password.text);
        if (u == null)
        {
            status.text = "Register failed (user exists or empty).";
            return;
        }

        status.text = "Registered! Logging in...";
        // burada GameManager varsa set edersin
        SceneManager.LoadScene("MainMenu"); // veya "Level_City"
    }

    private void DoLogin()
    {
        var u = userRepo.Login(username.text, password.text);
        if (u == null)
        {
            status.text = "Login failed (wrong username/password).";
            return;
        }

        status.text = "Login OK. Loading...";
        SceneManager.LoadScene("MainMenu"); // veya "Level_City"
    }
}

// ---------- küçük UI helper ----------
public static class UI
{
    public static RectTransform Box(Transform parent, string name, Vector2 size, Vector2 anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.35f);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    public static Text Label(RectTransform parent, string name, string text, int fontSize, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;
        t.text = text;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(480, 40);
        rt.anchoredPosition = pos;
        return t;
    }

    public static InputField Input(RectTransform parent, string name, string placeholder, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.white;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(420, 40);
        rt.anchoredPosition = pos;

        var input = go.AddComponent<InputField>();

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = "";
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleLeft;

        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(1, 1);
        trt.offsetMin = new Vector2(10, 6);
        trt.offsetMax = new Vector2(-10, -6);

        // Placeholder
        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(go.transform, false);
        var ph = phGO.AddComponent<Text>();
        ph.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        ph.text = placeholder;
        ph.color = new Color(0, 0, 0, 0.35f);
        ph.alignment = TextAnchor.MiddleLeft;

        var phrt = phGO.GetComponent<RectTransform>();
        phrt.anchorMin = new Vector2(0, 0);
        phrt.anchorMax = new Vector2(1, 1);
        phrt.offsetMin = new Vector2(10, 6);
        phrt.offsetMax = new Vector2(-10, -6);

        input.textComponent = text;
        input.placeholder = ph;

        return input;
    }

    public static Button Button(RectTransform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.55f, 0.95f, 1f);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(180, 44);
        rt.anchoredPosition = pos;

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        var tGO = new GameObject("Text");
        tGO.transform.SetParent(go.transform, false);
        var t = tGO.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.text = text;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;

        var trt = tGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }
}
