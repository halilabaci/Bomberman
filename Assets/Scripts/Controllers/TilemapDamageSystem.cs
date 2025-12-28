using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models.Map;

namespace DPBomberman.Controllers
{
    public class TilemapDamageSystem : MonoBehaviour
    {
        [Header("Tilemaps (Assign or Auto-Find)")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;      // Unbreakable
        public Tilemap breakableTilemap;  // Breakable
        public Tilemap hardTilemap;       // Hard (multi-hit)

        [Header("Explosion")]
        [Tooltip("Explosion tehlike süresi (ExplosionAreaTracker buna göre işaretleyebilir).")]
        public float dangerDuration = 0.35f;

        [Tooltip("Hard tile kaç patlamada kırılır?")]
        public int hardHitsToBreak = 2;

        [Header("Refs (optional)")]
        public ExplosionAreaTracker explosionTracker;

        // Hard tile hasar takibi
        private readonly Dictionary<Vector3Int, int> hardHits = new();

        private bool IsServerNow
            => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && NetworkManager.Singleton.IsServer;

        private void Awake()
        {
            AutoAssignRefs();
        }

        private void OnEnable()
        {
            AutoAssignRefs();
        }

        private void AutoAssignRefs()
        {
            AutoAssignTilemaps();

            if (explosionTracker == null)
                explosionTracker = Object.FindFirstObjectByType<ExplosionAreaTracker>();
        }

        private void AutoAssignTilemaps()
        {
            // ✅ senin istediğin: TilemapFinder ile güncel bağlama
            if (groundTilemap == null) groundTilemap = TilemapFinder.Find("Ground");
            if (solidTilemap == null) solidTilemap = TilemapFinder.Find("Walls_Solid");
            if (breakableTilemap == null) breakableTilemap = TilemapFinder.Find("Walls_Breakable");
            if (hardTilemap == null) hardTilemap = TilemapFinder.Find("Walls_Hard");
        }

        /// <summary>
        /// Bombanın patlamasını uygular: tile kırma + tehlike alanı + server kill.
        /// </summary>
        public void Explode(Vector3Int origin, int range, MapLogicAdapter mapLogicAdapter = null)
        {
            AutoAssignRefs();

            if (groundTilemap == null)
            {
                Debug.LogError("[TilemapDamageSystem] groundTilemap NULL. Explosion cancelled.");
                return;
            }

            range = Mathf.Max(1, range);

            // Patlama hücreleri
            var cells = new List<Vector3Int> { origin };

            // 4 yöne ışın (Bomberman)
            CollectRay(cells, origin, Vector3Int.right, range);
            CollectRay(cells, origin, Vector3Int.left, range);
            CollectRay(cells, origin, Vector3Int.up, range);
            CollectRay(cells, origin, Vector3Int.down, range);

            // Önce kırma uygulansın (server tarafı mantıklı)
            if (IsServerNow)
            {
                ApplyTileDamage(cells, mapLogicAdapter);
                KillPlayersInCells(cells);
            }

            // Tehlike alanı işaretle (client’larda da görsel/kill check için)
            ActivateDangerCellsSafe(cells, dangerDuration);
        }

        // --------------------------------------------------------------------
        // RAY COLLECT
        // --------------------------------------------------------------------
        private void CollectRay(List<Vector3Int> cells, Vector3Int origin, Vector3Int dir, int range)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector3Int c = origin + dir * i;

                // ground yoksa harita dışı say
                if (groundTilemap != null && !groundTilemap.HasTile(c)) break;

                // Unbreakable duvar patlamayı keser
                if (solidTilemap != null && solidTilemap.HasTile(c))
                    break;

                // Bu hücre patlama alanına girer
                cells.Add(c);

                // Breakable / Hard patlamayı durdurur (Bomberman kuralı)
                if (hardTilemap != null && hardTilemap.HasTile(c)) break;
                if (breakableTilemap != null && breakableTilemap.HasTile(c)) break;
            }
        }

        // --------------------------------------------------------------------
        // TILE DAMAGE
        // --------------------------------------------------------------------
        private void ApplyTileDamage(List<Vector3Int> cells, MapLogicAdapter mapLogicAdapter)
        {
            if (breakableTilemap == null && hardTilemap == null) return;

            MapGrid grid = null;
            if (mapLogicAdapter != null)
                grid = mapLogicAdapter.GetMapGrid();

            foreach (var c in cells)
            {
                // Solid hiçbir zaman kırılmasın
                if (solidTilemap != null && solidTilemap.HasTile(c))
                    continue;

                // Hard varsa önce onu işle
                if (hardTilemap != null && hardTilemap.HasTile(c))
                {
                    int hit = 0;
                    hardHits.TryGetValue(c, out hit);
                    hit++;
                    hardHits[c] = hit;

                    if (hit >= Mathf.Max(1, hardHitsToBreak))
                    {
                        hardTilemap.SetTile(c, null);
                        hardHits.Remove(c);

                        // Logic update
                        if (grid != null)
                            grid.SetCell(c.x, c.y, CellType.Ground);
                    }

                    // Hard, ray’i kesen engel olduğu için burada bitiriyoruz
                    continue;
                }

                // Breakable kır
                if (breakableTilemap != null && breakableTilemap.HasTile(c))
                {
                    breakableTilemap.SetTile(c, null);

                    // Logic update
                    if (grid != null)
                        grid.SetCell(c.x, c.y, CellType.Ground);

                    continue;
                }
            }
        }

        // --------------------------------------------------------------------
        // SERVER KILL
        // --------------------------------------------------------------------
        private void KillPlayersInCells(List<Vector3Int> cells)
        {
            if (!IsServerNow) return;
            if (groundTilemap == null) return;

            var players = Object.FindObjectsByType<DPBomberman.Controllers.PlayerController>(FindObjectsSortMode.None);

            foreach (var p in players)
            {
                if (p == null) continue;

                Vector3Int pCell = groundTilemap.WorldToCell(p.transform.position);

                if (!cells.Contains(pCell)) continue;

                // Öncelik: PlayerController.KillClientRpc varsa onu çağır
                if (!TryInvokeMethod(p, "KillClientRpc"))
                {
                    // Fallback: DamageableActor varsa Kill
                    var actor = p.GetComponent<DamageableActor>();
                    if (actor != null)
                    {
                        actor.Kill();
                        continue;
                    }

                    // Fallback: PlayerCollisionKill varsa Die
                    var kill = p.GetComponent<PlayerCollisionKill>();
                    if (kill != null)
                    {
                        kill.Die("Explosion");
                        continue;
                    }
                }
            }
        }

        // --------------------------------------------------------------------
        // DANGER TRACKER SAFE CALL (signature farklarına dayanıklı)
        // --------------------------------------------------------------------
        private void ActivateDangerCellsSafe(List<Vector3Int> cells, float duration)
        {
            if (explosionTracker == null) return;

            // Bazı projelerde: ActivateCells(List<Vector3Int>)
            // Bazılarında: ActivateCells(List<Vector3Int>, float)
            // Reflection ile ikisine de uyumlu çalıştırıyoruz.
            var t = explosionTracker.GetType();

            // 2 parametreli dene
            var m2 = t.GetMethod("ActivateCells",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(List<Vector3Int>), typeof(float) },
                null);

            if (m2 != null)
            {
                m2.Invoke(explosionTracker, new object[] { cells, duration });
                return;
            }

            // 1 parametreli dene
            var m1 = t.GetMethod("ActivateCells",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(List<Vector3Int>) },
                null);

            if (m1 != null)
            {
                m1.Invoke(explosionTracker, new object[] { cells });
                return;
            }

            Debug.LogWarning("[TilemapDamageSystem] ExplosionAreaTracker.ActivateCells method not found (signature mismatch).");
        }

        private static bool TryInvokeMethod(object target, string methodName)
        {
            if (target == null) return false;

            var m = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (m == null) return false;

            if (m.GetParameters().Length == 0)
            {
                m.Invoke(target, null);
                return true;
            }

            return false;
        }
    }
}
