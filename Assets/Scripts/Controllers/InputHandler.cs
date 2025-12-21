using UnityEngine;
using DPBomberman.Controllers;
using DPBomberman.Commands;

namespace DPBomberman.InputSystem
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Receiver")]
        [SerializeField] private PlayerController player;

        [Header("State Gate")]
        [SerializeField] private bool inputEnabled = true;

        private ICommand moveUp;
        private ICommand moveDown;
        private ICommand moveLeft;
        private ICommand moveRight;
        private ICommand placeBomb;

        private void Awake()
        {
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();
            BuildCommands();
        }

        private void BuildCommands()
        {
            if (player == null)
            {
                Debug.LogError("[InputHandler] PlayerController not found. Assign in Inspector.");
                enabled = false;
                return;
            }

            moveUp = new MoveUpCommand(player);
            moveDown = new MoveDownCommand(player);
            moveLeft = new MoveLeftCommand(player);
            moveRight = new MoveRightCommand(player);
            placeBomb = new PlaceBombCommand(player);
        }

        public void SetInputEnabled(bool enabledValue)
        {
            inputEnabled = enabledValue;
        }

        private void Update()
        {
            if (!inputEnabled) return;
            if (player == null) return;

            // Player öldüyse input da çalýþmasýn
            if (player.IsDead()) return;

            // Hareket: tek basýþ (GetKeyDown) - senin grid hareketin için doðru
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                moveUp.Execute();
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                moveDown.Execute();
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                moveLeft.Execute();
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                moveRight.Execute();

            // Bomba: tek basýþ
            if (Input.GetKeyDown(KeyCode.Space))
                placeBomb.Execute();
        }
    }
}
