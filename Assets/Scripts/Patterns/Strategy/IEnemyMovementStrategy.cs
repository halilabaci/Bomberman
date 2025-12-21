using UnityEngine;

namespace DPBomberman.Patterns.Strategy
{
    public interface IEnemyMovementStrategy
    {
        Vector2Int GetNextMove(EnemyContext context);
    }
}