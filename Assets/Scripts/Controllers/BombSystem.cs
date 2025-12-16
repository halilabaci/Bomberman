using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Controllers;

namespace DPBomberman.Controllers
{
    public class BombSystem : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject bombPrefab;

        [Header("Refs")]
        public PlayerController player;
        public Tilemap groundTilemap;
        public TilemapDamageSystem damageSystem;
        public MapLogicAdapter mapLogicAdapter;

        [Header("Settings")]
        public float fuseSeconds = 2.0f;
        public int range = 2;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlaceBomb();
            }
        }

        private void PlaceBomb()
        {
            if (bombPrefab == null)
            {
                Debug.LogError("[BombSystem] bombPrefab is NULL.");
                return;
            }
            if (player == null)
            {
                Debug.LogError("[BombSystem] player is NULL.");
                return;
            }
            if (groundTilemap == null)
            {
                Debug.LogError("[BombSystem] groundTilemap is NULL.");
                return;
            }
            if (damageSystem == null)
            {
                Debug.LogError("[BombSystem] damageSystem is NULL.");
                return;
            }

            Vector3Int cell = player.GetCurrentCell();

            GameObject bombObj = Instantiate(bombPrefab);
            var bomb = bombObj.GetComponent<BombController>();
            if (bomb == null)
            {
                Debug.LogError("[BombSystem] bombPrefab must have BombController component.");
                Destroy(bombObj);
                return;
            }

            bomb.fuseSeconds = fuseSeconds;
            bomb.range = range;
            bomb.groundTilemap = groundTilemap;
            bomb.damageSystem = damageSystem;
            bomb.mapLogicAdapter = mapLogicAdapter;

            bomb.Arm(cell);
        }
    }
}
