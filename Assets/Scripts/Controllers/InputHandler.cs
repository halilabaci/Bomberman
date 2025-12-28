using UnityEngine;
using DPBomberman.Controllers;
using DPBomberman.Commands;
using System.Collections;
using System.Collections.Generic;

namespace DPBomberman.InputSystem
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Key Bindings")]
        public KeyCode upKey = KeyCode.W;
        public KeyCode downKey = KeyCode.S;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode bombKey = KeyCode.Space;
        public KeyCode replayKey = KeyCode.R;

        [Header("Receiver")]
        [SerializeField] private PlayerController player;

        [Header("State Gate")]
        [SerializeField] private bool inputEnabled = true;

        private List<ICommand> commandHistory = new List<ICommand>();
        private bool isReplaying = false;
        private Vector3 startPosition;

        private ICommand moveUp;
        private ICommand moveDown;
        private ICommand moveLeft;
        private ICommand moveRight;
        private ICommand placeBomb;

        private void Awake()
        {
            TryBindLocalPlayer();
        }

        private PlayerController FindLocalPlayer()
        {
            var players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                if (p != null && p.IsOwner) return p;
            }
            return null;
        }

        private void TryBindLocalPlayer()
        {
            if (player != null && player.IsOwner) return;

            player = FindLocalPlayer();
            if (player == null) return;

            startPosition = player.transform.position;
            BuildCommands();
        }

        private void BuildCommands()
        {
            if (player == null) return;

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

        private void ExecuteAndRecord(ICommand command)
        {
            command.Execute();
            commandHistory.Add(command);
        }

        private void Update()
        {
            // Sahne değişince player spawn gecikebilir → her frame bağlanmayı dene
            if (player == null || !player.IsOwner)
                TryBindLocalPlayer();

            if (player == null) return;

            if (isReplaying) return;
            if (!inputEnabled) return;
            if (player.IsDead()) return;

            if (Input.GetKeyDown(replayKey))
            {
                StartCoroutine(StartReplayRoutine());
                return;
            }

            if (moveUp != null && (Input.GetKeyDown(upKey) || Input.GetKeyDown(KeyCode.UpArrow)))
                ExecuteAndRecord(moveUp);
            else if (moveDown != null && (Input.GetKeyDown(downKey) || Input.GetKeyDown(KeyCode.DownArrow)))
                ExecuteAndRecord(moveDown);
            else if (moveLeft != null && (Input.GetKeyDown(leftKey) || Input.GetKeyDown(KeyCode.LeftArrow)))
                ExecuteAndRecord(moveLeft);
            else if (moveRight != null && (Input.GetKeyDown(rightKey) || Input.GetKeyDown(KeyCode.RightArrow)))
                ExecuteAndRecord(moveRight);

            if (placeBomb != null && Input.GetKeyDown(bombKey))
                ExecuteAndRecord(placeBomb);
        }

        private IEnumerator StartReplayRoutine()
        {
            isReplaying = true;

            // Sahnedeki bombaları temizle (Replay’de çakışma olmasın)
            var activeBombs = Object.FindObjectsByType<BombController>(FindObjectsSortMode.None);
            foreach (var b in activeBombs)
                if (b != null) Destroy(b.gameObject);

            player.transform.position = startPosition;
            yield return new WaitForSeconds(0.5f);

            foreach (ICommand cmd in commandHistory)
            {
                cmd.Execute();
                yield return new WaitForSeconds(0.2f);
            }

            isReplaying = false;
            commandHistory.Clear();
        }
    }
}
