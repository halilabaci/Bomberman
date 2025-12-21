using UnityEngine;
using DPBomberman.Controllers;

namespace DPBomberman.Commands
{
    public class MoveUpCommand : ICommand
    {
        private readonly PlayerController player;

        public MoveUpCommand(PlayerController player) => this.player = player;

        public void Execute()
        {
            if (player == null) return;
            player.TryMove(Vector3Int.up);
        }
    }
}
