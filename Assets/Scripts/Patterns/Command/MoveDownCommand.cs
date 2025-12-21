using UnityEngine;
using DPBomberman.Controllers;

namespace DPBomberman.Commands
{
    public class MoveDownCommand : ICommand
    {
        private readonly PlayerController player;

        public MoveDownCommand(PlayerController player) => this.player = player;

        public void Execute()
        {
            if (player == null) return;
            player.TryMove(Vector3Int.down);
        }
    }
}
