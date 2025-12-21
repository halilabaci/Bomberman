using UnityEngine;

namespace DPBomberman.Patterns.Strategy
{
    public class ChasePlayerStrategy : IEnemyMovementStrategy
    {
        public Vector2Int GetNextMove(EnemyContext context)
        {
            // Player yoksa dur
            if (context.PlayerPosition == Vector3.zero) return Vector2Int.zero;

            Vector3 diff = context.PlayerPosition - context.EnemyPosition;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                if (diff.x > 0) return Vector2Int.right;
                if (diff.x < 0) return Vector2Int.left;
            }
            else
            {
                if (diff.y > 0) return Vector2Int.up;
                if (diff.y < 0) return Vector2Int.down;
            }
            return Vector2Int.zero;
        }
    }
}