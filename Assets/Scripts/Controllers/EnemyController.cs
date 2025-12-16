using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Controllers
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

        [Header("AI")]
        [Tooltip("Her adým arasýnda bekleme (çok hýzlý karar deðiþtirmesin)")]
        public float moveInterval = 0.10f;
        private float nextMoveTime = 0f;

        [Header("Death")]
        public ExplosionAreaTracker explosionTracker;
        public DamageableActor actor;

        private bool isMoving;
        private Vector3Int currentCell;

        private void Start()
        {
            if (groundTilemap == null)
            {
                Debug.LogError("[EnemyController] groundTilemap is NULL.");
                enabled = false;
                return;
            }

            currentCell = groundTilemap.WorldToCell(transform.position);
            SnapToCell(currentCell);

            // Spawn duvarýn üstündeyse yakýndaki boþ hücreye kaydýr
            TryRelocateIfBlocked();

            if (actor == null)
                actor = GetComponent<DamageableActor>();

            Debug.Log($"[EnemyController] Using groundTilemap: {groundTilemap.name}, startCell={currentCell}");
        }

        private void Update()
        {
            if (actor != null && actor.IsDead) return;

            // Patlama alanýnda mý?
            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                actor?.Kill();
                return;
            }

            if (isMoving) return;

            // Çok sýk yön deðiþtirmesin
            if (Time.time < nextMoveTime) return;
            nextMoveTime = Time.time + moveInterval;

            Vector3Int dir = PickRandomDirection();
            Vector3Int target = currentCell + dir;

            // blokluysa birkaç deneme yap
            int attempts = 0;
            while (IsBlocked(target) && attempts < 8)
            {
                dir = PickRandomDirection();
                target = currentCell + dir;
                attempts++;
            }

            if (IsBlocked(target))
                return;

            StartCoroutine(MoveCellTo(target));
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
            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;

            // ground yoksa harita dýþý
            if (!groundTilemap.HasTile(cell)) return true;

            return false;
        }

        private bool TryRelocateIfBlocked()
        {
            if (!IsBlocked(currentCell))
                return true;

            Vector3Int[] neighbors =
            {
                currentCell + Vector3Int.right,
                currentCell + Vector3Int.left,
                currentCell + Vector3Int.up,
                currentCell + Vector3Int.down
            };

            foreach (var n in neighbors)
            {
                if (!IsBlocked(n))
                {
                    currentCell = n;
                    SnapToCell(currentCell);
                    Debug.Log($"[EnemyController] Spawn was blocked, relocated to {currentCell}");
                    return true;
                }
            }

            Debug.LogWarning($"[EnemyController] Spawn is blocked and no free neighbor found at {currentCell}");
            return false;
        }

        private IEnumerator MoveCellTo(Vector3Int targetCell)
        {
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
            transform.position = groundTilemap.GetCellCenterWorld(cell);
        }

        public Vector3Int GetCurrentCell() => currentCell;
    }
}
