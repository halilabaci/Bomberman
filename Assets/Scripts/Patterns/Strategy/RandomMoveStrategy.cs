using UnityEngine;

namespace DPBomberman.Patterns.Strategy
{
    public class RandomMoveStrategy : IEnemyMovementStrategy
    {
        public Vector2Int GetNextMove(EnemyContext context)
        {
            // 0-3 arasý rastgele sayý: Yukarý, Aþaðý, Sol, Sað
            int rand = Random.Range(0, 4);

            return rand switch
            {
                0 => Vector2Int.up,
                1 => Vector2Int.down,
                2 => Vector2Int.left,
                3 => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }
    }
}