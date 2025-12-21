using UnityEngine;
using DPBomberman.Controllers;

namespace DPBomberman.Commands
{
    public class MoveRightCommand : ICommand
    {
        private readonly PlayerController player;

        public MoveRightCommand(PlayerController player) => this.player = player;

        public void Execute()
        {
            if (player == null) return;
            player.TryMove(Vector3Int.right);
        }
    }
}
