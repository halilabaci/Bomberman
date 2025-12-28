using UnityEngine;
using UnityEngine.Tilemaps;
using Patterns.Decorator;
using System.Collections.Generic;
using Unity.Netcode;

namespace DPBomberman.Controllers
{
    public class BombSystem : NetworkBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject bombPrefab;

        [Header("Refs")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private TilemapDamageSystem damageSystem;
        [SerializeField] private MapLogicAdapter mapLogicAdapter;

        [Header("Settings")]
        [SerializeField] private float fuseSeconds = 2.0f;
        [SerializeField] private int defaultRange = 1;

        private PlayerStatsHolder stats;
        private readonly List<BombController> myBombs = new List<BombController>();

        private void Awake()
        {
            // BombSystem player prefab üzerinde olmalı:
            if (player == null) player = GetComponent<PlayerController>();
            if (stats == null && player != null) stats = player.GetComponent<PlayerStatsHolder>();
        }

        private void EnsureRefs()
        {
            if (damageSystem == null) damageSystem = Object.FindFirstObjectByType<TilemapDamageSystem>();
            if (mapLogicAdapter == null) mapLogicAdapter = Object.FindFirstObjectByType<MapLogicAdapter>();

            // Tilemap'i isim bağımlılığını azaltarak bul
            if (groundTilemap == null) groundTilemap = TilemapFinder.Find("Ground");
            if (groundTilemap == null) groundTilemap = TilemapFinder.Find("Tilemap"); // en son fallback

            if (stats == null && player != null) stats = player.GetComponent<PlayerStatsHolder>();
        }

        public bool TryPlaceBomb()
        {
            // Bombayı sadece Server (Host) oluşturur
            if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return false;
            if (bombPrefab == null) return false;

            EnsureRefs();

            if (player == null || groundTilemap == null || damageSystem == null)
            {
                Debug.LogWarning("[BombSystem] Missing refs. player/ground/damageSystem");
                return false;
            }

            myBombs.RemoveAll(b => b == null);

            int maxBombs = 1;
            int dynamicRange = Mathf.Max(1, defaultRange);

            if (stats != null)
            {
                maxBombs = Mathf.Max(1, stats.BombCount);
                dynamicRange = Mathf.Max(1, stats.BombPower);
            }

            if (myBombs.Count >= maxBombs) return false;

            // Server tarafında cell'i transform position'dan hesapla
            Vector3Int cell = groundTilemap.WorldToCell(player.transform.position);

            // Aynı hücrede bomba varsa koyma
            for (int i = 0; i < myBombs.Count; i++)
            {
                if (myBombs[i] == null) continue;
                Vector3Int bombCell = groundTilemap.WorldToCell(myBombs[i].transform.position);
                if (bombCell == cell) return false;
            }

            if (PowerUpRegistry.Has(cell)) return false;
            if (BombRegistry.Has(cell)) return false;

            // Spawn: pozisyonu önce ayarla
            var worldPos = groundTilemap.GetCellCenterWorld(cell);
            var bombObj = Instantiate(bombPrefab, worldPos, Quaternion.identity);

            // Network Spawn
            var netObj = bombObj.GetComponent<NetworkObject>();
            if (netObj != null) netObj.Spawn();

            var bomb = bombObj.GetComponent<BombController>();
            if (bomb == null)
            {
                if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                else Destroy(bombObj);
                return false;
            }

            bomb.fuseSeconds = fuseSeconds;
            bomb.range = dynamicRange;
            bomb.groundTilemap = groundTilemap;
            bomb.damageSystem = damageSystem;
            bomb.mapLogicAdapter = mapLogicAdapter;

            myBombs.Add(bomb);
            bomb.Arm(cell);

            return true;
        }
    }
}
