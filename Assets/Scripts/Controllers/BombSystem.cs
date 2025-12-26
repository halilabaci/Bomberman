using UnityEngine;
using UnityEngine.Tilemaps;
using Patterns.Decorator;
using System.Collections.Generic;
using Unity.Netcode; // <--- Network kodları için bu şart!

namespace DPBomberman.Controllers
{
    public class BombSystem : MonoBehaviour
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

        private void Awake() => AutoWire();

        private void AutoWire()
        {
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();
            if (damageSystem == null) damageSystem = Object.FindFirstObjectByType<TilemapDamageSystem>();
            if (mapLogicAdapter == null) mapLogicAdapter = Object.FindFirstObjectByType<MapLogicAdapter>();

            if (groundTilemap == null)
            {
                var groundGO = GameObject.Find("Ground");
                if (groundGO != null) groundTilemap = groundGO.GetComponent<Tilemap>();
            }

            if (player != null) stats = player.GetComponent<PlayerStatsHolder>();
        }

        public bool TryPlaceBomb()
        {
            // ✅ MULTIPLAYER KONTROLÜ: Bombayı sadece SERVER (Host) oluşturabilir.
            if (!NetworkManager.Singleton.IsServer) return false;

            if (bombPrefab == null) return false;

            if (player == null || groundTilemap == null || damageSystem == null)
                AutoWire();

            if (player == null || groundTilemap == null || damageSystem == null)
                return false;

            if (stats == null) stats = player.GetComponent<PlayerStatsHolder>();

            myBombs.RemoveAll(b => b == null);

            int maxBombs = 1;
            int dynamicRange = Mathf.Max(1, defaultRange);

            if (stats != null)
            {
                maxBombs = Mathf.Max(1, stats.BombCount);
                dynamicRange = Mathf.Max(1, stats.BombPower);
            }

            if (myBombs.Count >= maxBombs) return false;

            Vector3Int cell = player.GetCurrentCell();

            for (int i = 0; i < myBombs.Count; i++)
            {
                if (myBombs[i] == null) continue;
                Vector3Int bombCell = groundTilemap.WorldToCell(myBombs[i].transform.position);
                if (bombCell == cell) return false;
            }

            if (PowerUpRegistry.Has(cell)) return false;

            // ✅ SPAWN İŞLEMİ
            var bombObj = Instantiate(bombPrefab);

            // 🔥 SİHİRLİ DOKUNUŞ: Bombayı ağ üzerinde herkes için oluşturur!
            var netObj = bombObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }

            var bomb = bombObj.GetComponent<BombController>();
            if (bomb == null)
            {
                bombObj.GetComponent<NetworkObject>().Despawn(); // Hata varsa ağdan da sil
                return false;
            }

            bomb.fuseSeconds = fuseSeconds;
            bomb.range = dynamicRange;
            bomb.groundTilemap = groundTilemap;
            bomb.damageSystem = damageSystem;
            bomb.mapLogicAdapter = mapLogicAdapter;

            myBombs.Add(bomb);
            bomb.Arm(cell);

            StartCoroutine(RemoveAfterFuse(bomb, fuseSeconds + 0.25f));

            return true;
        }

        private System.Collections.IEnumerator RemoveAfterFuse(BombController bomb, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (bomb != null) myBombs.Remove(bomb);
        }
    }
}