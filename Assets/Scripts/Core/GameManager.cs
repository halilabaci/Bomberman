using UnityEngine;
using UnityEngine.SceneManagement;
using DPBomberman.Patterns.State;
using DPBomberman.Patterns.Factory;
using DPBomberman.Models;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UIManager uiManager;

    [Header("Scene References (auto-bind)")]
    public MapGenerator mapGenerator;
    public MapLogicAdapter mapLogicAdapter;

    [Header("Input")]
    public DPBomberman.InputSystem.InputHandler inputHandler;

    [Header("Theme")]
    public ThemeType selectedTheme = ThemeType.Forest;
    public WallTileThemeFactorySO[] availableThemeFactories;

    public IWallTileFactory CurrentTileFactory { get; private set; }

    private GameStateMachine stateMachine;
    private bool bootstrapped;

    private void Awake()
    {
        // --- Singleton ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);

        // --- State Machine ---
        stateMachine = new GameStateMachine();

        // Scene event aboneliği
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        ResolveThemeFactory();
    }

    public void SetGameplayInput(bool enabled)
    {
        if (inputHandler != null)
            inputHandler.SetInputEnabled(enabled);
    }

    private void Start()
    {
        // İlk sahne için manuel tetikleme (yalnızca bir kez)
        if (bootstrapped) return;
        bootstrapped = true;

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        stateMachine?.Tick(Time.deltaTime);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bootstrapped = true;

        Time.timeScale = 1f;

        BindSceneReferences();
        ResolveThemeFactory();

        var sceneName = scene.name.ToLowerInvariant();

        if (sceneName.Contains("mainmenu"))
            stateMachine.ChangeState(new MainMenuState(this, stateMachine));
        else
            stateMachine.ChangeState(new PlayingState(this, stateMachine));

        uiManager?.HideGameOver();
        uiManager?.HidePause();
    }

    private void BindSceneReferences()
    {
        mapGenerator = FindFirstObjectByType<MapGenerator>();
        mapLogicAdapter = FindFirstObjectByType<MapLogicAdapter>();
        uiManager = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);

        if (inputHandler == null)
            inputHandler = FindFirstObjectByType<DPBomberman.InputSystem.InputHandler>();

        var sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();

        if (uiManager == null && !sceneName.Contains("mainmenu"))
            Debug.LogWarning($"[GameManager] UIManager missing in scene: {sceneName}");
        else
            Debug.Log($"[GameManager] Bind UI => ui={(uiManager ? uiManager.name : "NULL")}");

        Debug.Log($"[GameManager] Bind refs => mapGen={(mapGenerator ? mapGenerator.name : "NULL")}, mapLogic={(mapLogicAdapter ? mapLogicAdapter.name : "NULL")}");
    }

    public void ResolveThemeFactory()
    {
        CurrentTileFactory = null;

        if (availableThemeFactories == null || availableThemeFactories.Length == 0)
        {
            Debug.LogError("[GameManager] availableThemeFactories boş. Inspector'da doldur.");
            return;
        }

        foreach (var f in availableThemeFactories)
        {
            if (f != null && f.Theme == selectedTheme)
            {
                CurrentTileFactory = f;
                Debug.Log($"[GameManager] Theme selected: {selectedTheme} -> Factory: {f.name}");
                return;
            }
        }

        Debug.LogError("[GameManager] Selected theme factory not found. availableThemeFactories kontrol et.");
    }

    public void SetTheme(ThemeType theme)
    {
        selectedTheme = theme;
        ResolveThemeFactory();
    }

    private string GetSceneNameForTheme(ThemeType t)
    {
        return t switch
        {
            ThemeType.City => "Level_City",
            ThemeType.Desert => "Level_Desert",
            ThemeType.Forest => "Level_Forest",
            _ => "Level_City"
        };
    }

    // ✅ GÜNCEL: Scene load tek yerden (Single'da SceneManager, Netcode varsa server)
    public void StartGame(ThemeType theme)
    {
        Time.timeScale = 1f;
        SetTheme(theme);
        LevelLoader.LoadLevel(GetSceneNameForTheme(theme));
    }

    public void RestartLevel()
    {
        Debug.Log("[GameManager] RestartLevel CALLED");
        Time.timeScale = 1f;
        LevelLoader.LoadLevel(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameManager] GoToMainMenu CALLED");
        Time.timeScale = 1f;
        LevelLoader.LoadLevel("MainMenu");
    }

    public void OnPlayerDied() => GoToGameOver();

    public void GoToGameOver()
    {
        stateMachine.ChangeState(new GameOverState(this, stateMachine));
    }

    public void ResumeFromPause()
    {
        Debug.Log("[GameManager] ResumeFromPause CALLED");
        Time.timeScale = 1f;
        uiManager?.HidePause();
    }
}
