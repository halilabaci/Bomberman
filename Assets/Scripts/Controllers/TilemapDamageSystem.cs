using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models.Map;
using DPBomberman.Patterns.Factory;

namespace DPBomberman.Controllers
{
    public class TilemapDamageSystem : MonoBehaviour
    {
        [Header("Tilemaps")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap; // varsa

        [Header("Hard Wall Settings")]
        public int hardWallHp = 3;

        [Header("Explosion Tracking")]
        public ExplosionAreaTracker explosionTracker;

        private TileWallFactory factory;

        // Hard tile HP: tile olduðu için runtime sözlükte tutuyoruz
        private readonly Dictionary<Vector3Int, int> hardHp = new();

        private void Awake()
        {   
            if (factory == null)
                factory = new TileWallFactory(solidTilemap, breakableTilemap, hardTilemap);


        public void Explode(Vector3Int origin, int range, MapLogicAdapter logicAdapter)
        {
            var cells = new List<Vector3Int>();
            cells.Add(origin);

            CollectRay(origin, Vector3Int.right, range, cells);
            CollectRay(origin, Vector3Int.left, range, cells);
            CollectRay(origin, Vector3Int.up, range, cells);
            CollectRay(origin, Vector3Int.down, range, cells);

            // Patlama alanýný kýsa süre "tehlikeli" yap (ölüm kontrolü buradan)
            explosionTracker?.ActivateCells(cells);

            // Merkez hücre
            ApplyExplosionToCell(origin, logicAdapter);

            // 4 yön
            Propagate(origin, Vector3Int.right, range, logicAdapter);
            Propagate(origin, Vector3Int.left, range, logicAdapter);
            Propagate(origin, Vector3Int.up, range, logicAdapter);
            Propagate(origin, Vector3Int.down, range, logicAdapter);

            Debug.Log($"[Explosion] origin={origin} range={range}");
        }

        private void Propagate(Vector3Int origin, Vector3Int dir, int range, MapLogicAdapter logicAdapter)
        {
            for (int step = 1; step <= range; step++)
            {
                Vector3Int cell = origin + dir * step;

                // Harita dýþýnda patlamayý kes
                if (groundTilemap != null && !groundTilemap.HasTile(cell))
                    break;

                bool stop = ApplyExplosionToCell(cell, logicAdapter);
                if (stop) break;
            }
        }

        private void CollectRay(Vector3Int origin, Vector3Int dir, int range, List<Vector3Int> cells)
        {
            for (int step = 1; step <= range; step++)
            {
                Vector3Int cell = origin + dir * step;
                // Harita dýþýnda patlamayý kes
                if (groundTilemap != null && !groundTilemap.HasTile(cell))
                    break;
                cells.Add(cell);
                CellType type = factory.GetCellType(cell);
                if (type == CellType.Unbreakable || type == CellType.Breakable || type == CellType.Hard)
                    break;
            }
        }


        // true dönerse patlama o yönde durur
        private bool ApplyExplosionToCell(Vector3Int cell, MapLogicAdapter logicAdapter)
        {
            CellType type = factory.GetCellType(cell);

            // Unbreakable: dur
            if (type == CellType.Unbreakable)
                return true;

            // Breakable: kýr + dur
            if (type == CellType.Breakable)
            {
                if (breakableTilemap != null)
                    breakableTilemap.SetTile(cell, null);

                // Logic map güncelle
                logicAdapter?.GetMapGrid()?.SetCell(cell.x, cell.y, CellType.Ground);

                return true;
            }

            // Hard: HP düþ + (0 ise kýr) + dur
            if (type == CellType.Hard && hardTilemap != null)
            {
                int hpLeft = ApplyHardDamage(cell);

                if (hpLeft <= 0)
                {
                    hardTilemap.SetTile(cell, null);
                    logicAdapter?.GetMapGrid()?.SetCell(cell.x, cell.y, CellType.Ground);
                }

                return true;
            }

            // Ground/Empty: durma yok
            return false;
        }

        private int ApplyHardDamage(Vector3Int cell)
        {
            if (!hardHp.TryGetValue(cell, out int hp))
            {
                hp = hardWallHp;
            }

            hp -= 1;

            if (hp <= 0)
            {
                hardHp.Remove(cell);
                return 0;
            }

            hardHp[cell] = hp;
            return hp;
        }
    }
}
