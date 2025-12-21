using DPBomberman.Controllers;

namespace DPBomberman.Commands
{
    public class PlaceBombCommand : ICommand
    {
        private readonly PlayerController player;
        public PlaceBombCommand(PlayerController player) => this.player = player;

        public void Execute()
        {
            if (player == null) return;
            player.TryPlaceBomb();
        }
    }
}
