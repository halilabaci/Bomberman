using UnityEngine;
using UnityEngine.SceneManagement;
using DPBomberman.Patterns.State;


public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    public MapGenerator mapGenerator;
    public MapLogicAdapter mapLogicAdapter;
    [Header("Input")]
    public DPBomberman.InputSystem.InputHandler inputHandler;

    private GameStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new GameStateMachine();
    }
    public void SetGameplayInput(bool enabled)
    {
        if (inputHandler != null)
            inputHandler.SetInputEnabled(enabled);
    }


    private void Start()
    {
        var scene = SceneManager.GetActiveScene().name;

        if (scene == "MainMenu")
            stateMachine.ChangeState(new MainMenuState(this, stateMachine));
        else
            stateMachine.ChangeState(new PlayingState(this, stateMachine)); // ✅ otomatik başla
    }

    private void Update()
    {
        stateMachine.Tick(Time.deltaTime);
    }
}