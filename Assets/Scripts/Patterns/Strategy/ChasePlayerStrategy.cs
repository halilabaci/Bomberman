using UnityEngine;

namespace DPBomberman.Patterns.Strategy
{
    public class ChasePlayerStrategy : IEnemyMovementStrategy
    {
        public Vector2Int GetNextMove(EnemyContext context)
        {
            // Oyuncu ile düþman arasýndaki farký al
            Vector3 diff = context.PlayerPosition - context.EnemyPosition;

            // Eðer X farký Y farkýndan büyükse, önce yatayda yaklaþmaya çalýþ
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            {
                if (diff.x > 0) return Vector2Int.right; // Oyuncu saðda
                if (diff.x < 0) return Vector2Int.left;  // Oyuncu solda
            }
            else
            {
                // Deðilse dikeyde yaklaþ
                if (diff.y > 0) return Vector2Int.up;    // Oyuncu yukarýda
                if (diff.y < 0) return Vector2Int.down;  // Oyuncu aþaðýda
            }

            // Tam üstündeyse hareket etme (veya rastgele bir yön verilebilir)
            return Vector2Int.zero;
        }
    }
}