using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
// Kendi klasöründe olduðu için "using DPBomberman.Patterns.Strategy;" GEREKMEZ.

namespace DPBomberman.Patterns.Strategy // <-- ARTIK BURASI DA STRATEGY OLDU
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Tooltip("Bir hücreden diðerine geçiþ süresi")]
        public float stepDuration = 0.14f;

        [Header("AI Timing")]
        public float moveInterval = 0.10f;
        private float nextMoveTime = 0f;

        [Header("AI Strategy")]
        public EnemyStrategyType strategyType = EnemyStrategyType.Random; // <-- ARTIK HATA VERMEZ

        public Transform player;
        private IEnemyMovementStrategy strategy; // <-- ARTIK HATA VERMEZ

        [Header("Death")]
        // Bu sýnýflar baþka yerdeyse hata verebilir, aþaðýda açýklamasýný yaptým*
        public DPBomberman.Controllers.ExplosionAreaTracker explosionTracker;
        public DPBomberman.Controllers.DamageableActor actor;

        private bool isMoving;
        private Vector3Int currentCell;

        public void InjectTilemaps(Tilemap ground, Tilemap solid, Tilemap breakable, Tilemap hard)
        {
            groundTilemap = ground;
            solidTilemap = solid;
            breakableTilemap = breakable;
            hardTilemap = hard;
        }

        public void SetStrategy(EnemyStrategyType type)
        {
            strategyType = type;
            strategy = CreateStrategy(strategyType);
        }

        private void Start()
        {
            if (actor == null) actor = GetComponent<DPBomberman.Controllers.DamageableActor>();
            if (explosionTracker == null) explosionTracker = FindFirstObjectByType<DPBomberman.Controllers.ExplosionAreaTracker>();

            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            EnsureTilemapsBound();
            if (groundTilemap == null)
            {
                enabled = false;
                return;
            }

            strategy = CreateStrategy(strategyType);
            currentCell = groundTilemap.WorldToCell(transform.position);
            SnapToCell(currentCell);
            TryRelocateIfBlocked();
        }

        private void Update()
        {
            if (!enabled || groundTilemap == null) return;
            if (actor != null && actor.IsDead) return;

            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                if (actor != null) actor.Kill();
                return;
            }

            if (isMoving) return;
            if (Time.time < nextMoveTime) return;
            nextMoveTime = Time.time + moveInterval;

            Vector3Int dir = PickDirectionFromStrategy();
            Vector3Int target = currentCell + dir;

            int attempts = 0;
            while (IsBlocked(target) && attempts < 8)
            {
                dir = PickDirectionFromStrategy();
                target = currentCell + dir;
                attempts++;
            }

            if (IsBlocked(target)) return;

            StartCoroutine(MoveCellTo(target));
        }

        private Vector3Int PickDirectionFromStrategy()
        {
            if (strategy == null || player == null) return PickRandomDirection();

            var ctx = new EnemyContext(
                transform.position,
                player.position,
                transform.up,
                Time.deltaTime
            );

            Vector2Int move = strategy.GetNextMove(ctx);
            if (move == Vector2Int.zero) return PickRandomDirection();

            return new Vector3Int(move.x, move.y, 0);
        }

        private IEnemyMovementStrategy CreateStrategy(EnemyStrategyType type)
        {
            return type switch
            {
                EnemyStrategyType.Random => new RandomMoveStrategy(),
                EnemyStrategyType.ChasePlayer => new ChasePlayerStrategy(),
                _ => new RandomMoveStrategy()
            };
        }

        private void EnsureTilemapsBound()
        {
            if (groundTilemap == null) groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();
            if (solidTilemap == null) solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();
            if (breakableTilemap == null) breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();
            if (hardTilemap == null) hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();
        }

        private Vector3Int PickRandomDirection()
        {
            int r = Random.Range(0, 4);
            return r switch
            {
                0 => Vector3Int.right,
                1 => Vector3Int.left,
                2 => Vector3Int.up,
                _ => Vector3Int.down,
            };
        }

        private bool IsBlocked(Vector3Int cell)
        {
            if (groundTilemap == null) return true;
            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;
            if (!groundTilemap.HasTile(cell)) return true;
            return false;
        }

        private bool TryRelocateIfBlocked()
        {
            if (!IsBlocked(currentCell)) return true;
            Vector3Int[] neighbors = { currentCell + Vector3Int.right, currentCell + Vector3Int.left, currentCell + Vector3Int.up, currentCell + Vector3Int.down };
            foreach (var n in neighbors)
            {
                if (!IsBlocked(n))
                {
                    currentCell = n;
                    SnapToCell(currentCell);
                    return true;
                }
            }
            return false;
        }

        private IEnumerator MoveCellTo(Vector3Int targetCell)
        {
            if (groundTilemap == null) yield break;
            isMoving = true;
            Vector3 startPos = transform.position;
            Vector3 targetPos = groundTilemap.GetCellCenterWorld(targetCell);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.001f, stepDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            currentCell = targetCell;
            SnapToCell(currentCell);
            isMoving = false;
        }

        private void SnapToCell(Vector3Int cell)
        {
            if (groundTilemap == null) return;
            transform.position = groundTilemap.GetCellCenterWorld(cell);
        }

        public Vector3Int GetCurrentCell() => currentCell;
    }
}