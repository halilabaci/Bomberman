using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Controllers
{
    public class BombController : MonoBehaviour
    {
        [Header("Bomb Settings")]
        public float fuseSeconds = 2.0f;
        public int range = 2;

        [Header("Refs")]
        public Tilemap groundTilemap;
        public TilemapDamageSystem damageSystem;
        public MapLogicAdapter mapLogicAdapter;

        private Vector3Int originCell;

        public void Arm(Vector3Int cell)
        {
            originCell = cell;

            if (groundTilemap != null)
                transform.position = groundTilemap.GetCellCenterWorld(cell);

            StartCoroutine(FuseRoutine());
        }

        private IEnumerator FuseRoutine()
        {
            yield return new WaitForSeconds(fuseSeconds);

            if (damageSystem == null)
            {
                Debug.LogError("[BombController] damageSystem is NULL.");
                Destroy(gameObject);
                yield break;
            }

            damageSystem.Explode(originCell, range, mapLogicAdapter);

            Destroy(gameObject);
        }
    }
}
