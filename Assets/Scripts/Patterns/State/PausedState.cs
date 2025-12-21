using UnityEngine;

namespace DPBomberman.Patterns.State
{
    public class PausedState : IGameState
    {   
        private readonly GameManager game;
        private readonly GameStateMachine machine;

        public PausedState(GameManager game, GameStateMachine machine)
        {   
            this.game = game;
            this.machine = machine;
        }

        public void Enter()
        {
            Debug.Log("[STATE] Enter Paused");
            // TODO (Faz 2+): Time.timeScale = 0 veya input kilitleme
            game.SetGameplayInput(false);
            Time.timeScale = 0f;
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit Paused");
            // TODO (Faz 2+): Time.timeScale = 1
            Time.timeScale = 1f;
        }

        public void Tick(float deltaTime)
        {
            // Test amaçlý: ESC tekrar basýnca oyuna dön
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                machine.ChangeState(new PlayingState(game, machine));
            }

            // Test amaçlý: M ile menüye dön
            if (Input.GetKeyDown(KeyCode.M))
            {
                machine.ChangeState(new MainMenuState(game, machine));
            }
        }
    }
}