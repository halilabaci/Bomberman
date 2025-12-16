using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;          // World<->Cell dönüþümü için
        public Tilemap solidTilemap;           // Unbreakable duvarlar
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;// Hard duvar katmaný// Breakable duvarlar (istersen bloklamasýn)

        [Header("Death")]
        public ExplosionAreaTracker explosionTracker;
        public DamageableActor actor;

        [Tooltip("Bir hücreden diðerine geçiþ süresi (sn). 0.08-0.15 arasý iyi.)")]
        public float stepDuration = 0.12f;

        private bool isMoving;
        private Vector3Int currentCell;

        private void Start()
        {
            if (groundTilemap == null)
            {
                Debug.LogError("[PlayerController] groundTilemap is NULL. Assign in Inspector.");
                enabled = false;
                return;
            }

            // Baþlangýç hücresi
            currentCell = groundTilemap.WorldToCell(transform.position);
            SnapToCell(currentCell);

            if (actor == null)
                actor = GetComponent<DamageableActor>();

        }

        private void Update()
        {
            // 1) Öldüyse hiç iþlem yapma
            if (actor != null && actor.IsDead) return;

            // 2) Patlama hücresindeyse öl
            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                actor?.Kill();
                return;
            }

            // 3) Normal hareket kontrolü
            if (isMoving) return;

            Vector3Int dir = ReadDirection();
            if (dir == Vector3Int.zero) return;

            Vector3Int targetCell = currentCell + dir;

            if (IsBlocked(targetCell))
                return;

            StartCoroutine(MoveCellTo(targetCell));
        }

        private Vector3Int ReadDirection()
        {
            // Öncelik: yatay, sonra dikey (ayný anda basýlýrsa)
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                return new Vector3Int(-1, 0, 0);

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                return new Vector3Int(1, 0, 0);

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                return new Vector3Int(0, 1, 0);

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                return new Vector3Int(0, -1, 0);

            return Vector3Int.zero;
        }

        private bool IsBlocked(Vector3Int cell)
        {
            // Solid duvar varsa blokla
            if (solidTilemap != null && solidTilemap.HasTile(cell))
                return true;

            if (hardTilemap != null && hardTilemap.HasTile(cell))
                return true;
            
            // Breakable duvar varsa blokla (klasik bomberman gibi)
            if (breakableTilemap != null && breakableTilemap.HasTile(cell))
                return true;

            // Ground tile yoksa (harita dýþý) blokla
            if (!groundTilemap.HasTile(cell))
                return true;

            return false;
        }

        private System.Collections.IEnumerator MoveCellTo(Vector3Int targetCell)
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

        // Faz 1–2 için yardýmcý: baþka sistemler player cell'ini almak isterse
        public Vector3Int GetCurrentCell() => currentCell;
    }
}
