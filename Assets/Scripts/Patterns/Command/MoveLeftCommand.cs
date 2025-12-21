using UnityEngine;
using DPBomberman.Controllers;

namespace DPBomberman.Commands
{
    public class MoveLeftCommand : ICommand
    {
        private readonly PlayerController player;

        public MoveLeftCommand(PlayerController player) => this.player = player;

        public void Execute()
        {
            if (player == null) return;
            player.TryMove(Vector3Int.left);
        }
    }
}
