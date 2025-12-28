using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

namespace DPBomberman.Controllers
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Header("Bomb")]
        [SerializeField] private BombSystem bombSystem;

        [Header("Death")]
        public ExplosionAreaTracker explosionTracker;
        public DamageableActor actor;

        [Tooltip("Bir hücreden diğerine geçiş süresi (sn). 0.08-0.15 arası iyi.")]
        public float stepDuration = 0.12f;

        private bool isMoving;
        private Vector3Int currentCell;

        private float baseStepDuration;

        private void Awake()
        {
            if (actor == null) actor = GetComponent<DamageableActor>();
            if (bombSystem == null) bombSystem = GetComponent<BombSystem>(); // ✅ yanlış find kaldırıldı

            baseStepDuration = stepDuration;

            // ✅ ÇAKIŞMA ÖNLE: Rigidbody tabanlı PlayerMovement varsa kapat
            var rbMove = GetComponent<PlayerMovement>();
            if (rbMove != null) rbMove.enabled = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoadedRebind;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoadedRebind;
        }

        public override void OnNetworkSpawn()
        {
            // Player menu'de spawn olsa bile, level gelince rebind edeceğiz
            RebindTilemapsAndSnap();
            base.OnNetworkSpawn();
        }

        private void OnSceneLoadedRebind(Scene scene, LoadSceneMode mode)
        {
            RebindTilemapsAndSnap();
        }

        private void RebindTilemapsAndSnap()
        {
            BindTilemaps();

            if (groundTilemap != null)
            {
                currentCell = groundTilemap.WorldToCell(transform.position);
                SnapToCell(currentCell);
            }
        }

        private void BindTilemaps()
        {
            // Sahneye göre tilemap objeleri farklı hiyerarşide olabilir → TilemapFinder ile yakala
            if (groundTilemap == null) groundTilemap = TilemapFinder.Find("Ground");
            if (solidTilemap == null) solidTilemap = TilemapFinder.Find("Walls_Solid");
            if (breakableTilemap == null) breakableTilemap = TilemapFinder.Find("Walls_Breakable");
            if (hardTilemap == null) hardTilemap = TilemapFinder.Find("Walls_Hard");

            // Bazı sahnelerde isimler farklıysa "contains" yakalayacak
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (actor != null && actor.IsDead) return;

            // Tilemap null kalmışsa tekrar bağlanmayı dene (özellikle menu->level geçişi)
            if (groundTilemap == null) BindTilemaps();

            // (Opsiyonel) client-side tehlike kontrolü (asıl kill server’dan geliyor)
            if (explosionTracker != null && groundTilemap != null)
            {
                var cell = groundTilemap.WorldToCell(transform.position);
                currentCell = cell;

                if (explosionTracker.IsCellDangerous(currentCell))
                {
                    NotifyServerOfDeathServerRpc();
                }
            }
        }

        [ServerRpc]
        private void NotifyServerOfDeathServerRpc()
        {
            KillClientRpc();
        }

        [ClientRpc]
        public void KillClientRpc()
        {
            actor?.Kill();
        }

        public void TryMove(Vector3Int dir)
        {
            if (!IsOwner) return;
            if (actor != null && actor.IsDead) return;
            if (isMoving) return;
            if (dir == Vector3Int.zero) return;

            if (groundTilemap == null) BindTilemaps();
            if (groundTilemap == null) return;

            Vector3Int targetCell = currentCell + dir;
            if (IsBlocked(targetCell)) return;

            StartCoroutine(MoveCellTo(targetCell));
        }

        public void TryPlaceBomb()
        {
            if (!IsOwner) return;
            if (actor != null && actor.IsDead) return;

            PlaceBombServerRpc();
        }

        [ServerRpc]
        private void PlaceBombServerRpc()
        {
            if (bombSystem != null)
                bombSystem.TryPlaceBomb();
        }

        public bool IsDead()
        {
            return actor != null && actor.IsDead;
        }

        private bool IsBlocked(Vector3Int cell)
        {
            // Bombayı blok say
            if (BombRegistry.Has(cell)) return true;

            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;

            // ground yoksa yürünemez
            if (groundTilemap != null && !groundTilemap.HasTile(cell)) return true;

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
            if (groundTilemap != null)
                transform.position = groundTilemap.GetCellCenterWorld(cell);
        }

        public Vector3Int GetCurrentCell() => currentCell;
    }
}
