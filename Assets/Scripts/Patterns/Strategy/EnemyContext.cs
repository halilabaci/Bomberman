using UnityEngine;

namespace DPBomberman.Patterns.Strategy
{
    public class EnemyContext
    {
        public Vector3 EnemyPosition { get; private set; }
        public Vector3 PlayerPosition { get; private set; }
        public Vector3 CurrentDirection { get; private set; }
        public float DeltaTime { get; private set; }

        public EnemyContext(Vector3 enemyPos, Vector3 playerPos, Vector3 currentDir, float dt)
        {
            EnemyPosition = enemyPos;
            PlayerPosition = playerPos;
            CurrentDirection = currentDir;
            DeltaTime = dt;
        }
    }
}