using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using Patterns.Decorator;

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

        [Tooltip("Bir hücreden diğerine geçiş süresi (sn). 0.08-0.15 arası iyi.)")]
        public float stepDuration = 0.12f;

        private bool isMoving;
        private Vector3Int currentCell;

        private PlayerStatsHolder stats;
        private float baseStepDuration;

        private void Awake()
        {
            stats = GetComponent<PlayerStatsHolder>();
            if (bombSystem == null) bombSystem = Object.FindFirstObjectByType<BombSystem>();
            if (actor == null) actor = GetComponent<DamageableActor>();
            baseStepDuration = stepDuration; // Başlangıç hızını kaydet
        }

        private void Start()
        {
            // Sadece bu karakterin sahibiysen (senin karakterinse) bu işlemleri yap
            if (!IsOwner) return;

            // --- OTOMATİK BULMA SİSTEMİ ---
            if (groundTilemap == null)
                groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();

            if (solidTilemap == null)
                solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();

            if (breakableTilemap == null)
                breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();

            if (hardTilemap == null)
                hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();

            // Pozisyonu hücreye sabitle
            if (groundTilemap != null)
            {
                currentCell = groundTilemap.WorldToCell(transform.position);
                SnapToCell(currentCell);
            }
            else
            {
                Debug.LogError("DİKKAT: Sahnede 'Ground' isminde bir Tilemap bulunamadı!");
            }
        }

        private void Update()
        {
            // 1. Önce sadece sahibi olduğumuz karakterin tehlike durumuna bakalım
            if (!IsOwner) return;

            if (actor != null && actor.IsDead) return;

            // 2. Eğer bulunduğumuz hücre tehlikeliyse (bomba patladıysa)
            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                // Kendi kendimizi öldürmek yerine Sunucuya bildiriyoruz
                NotifyServerOfDeathServerRpc();
            }
        }

        [ServerRpc]
        private void NotifyServerOfDeathServerRpc()
        {
            // Sunucu (Host) ölümü onaylar ve herkese "Bu oyuncu öldü!" der
            HandleDeathClientRpc();
        }

        [ClientRpc]
        private void HandleDeathClientRpc()
        {
            // Bu fonksiyon her iki oyuncunun ekranında da aynı anda çalışır
            actor?.Kill();

            // Opsiyonel: Buraya Game Over UI'sını açacak kodu ekleyebilirsin
            Debug.Log("B-3 Senkronizasyonu: Bir oyuncu patlamada can verdi!");
        }

        public void TryMove(Vector3Int dir)
        {
            if (!IsOwner) return;
            if (actor != null && actor.IsDead) return;
            if (isMoving) return;
            if (dir == Vector3Int.zero) return;

            Vector3Int targetCell = currentCell + dir;
            if (IsBlocked(targetCell)) return;

            StartCoroutine(MoveCellTo(targetCell));
        }

        // ✅ COMMAND'lerin çağıracağı giriş noktası
        public void TryPlaceBomb()
        {
            if (!IsOwner) return; // Sadece sahibi basabilir
            if (actor != null && actor.IsDead) return;

            // Sunucuya "Bomba Koy" isteği gönderiyoruz
            PlaceBombServerRpc();
        }

        [ServerRpc]
        private void PlaceBombServerRpc()
        {
            // Sunucu bu isteği aldığında BombSystem üzerinden bombayı gerçekten oluşturur
            // Server tarafında çalıştığı için BombSystem içindeki IsServer kontrolünden geçer
            if (bombSystem != null)
            {
                bombSystem.TryPlaceBomb();
            }
        }

        public bool IsDead()
        {
            return actor != null && actor.IsDead;
        }

        private bool IsBlocked(Vector3Int cell)
        {
            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (groundTilemap != null && !groundTilemap.HasTile(cell)) return true;
            return false;
        }

        private System.Collections.IEnumerator MoveCellTo(Vector3Int targetCell)
        {
            isMoving = true;

            Vector3 startPos = transform.position;
            Vector3 targetPos = groundTilemap.GetCellCenterWorld(targetCell);

            float speedValue = (stats != null) ? Mathf.Max(0.1f, stats.Speed) : 1f;
            float effectiveStepDuration = baseStepDuration / speedValue;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.001f, effectiveStepDuration);
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