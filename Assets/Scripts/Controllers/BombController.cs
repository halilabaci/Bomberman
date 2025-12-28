using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Netcode;

namespace DPBomberman.Controllers
{
    public class BombController : NetworkBehaviour
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

            // pozisyonu cell center'a koy (server koyacak, herkes görecek)
            if (groundTilemap != null)
                transform.position = groundTilemap.GetCellCenterWorld(cell);

            // Bomba hücresi artýk blok
            BombRegistry.Register(originCell);

            // Fuse sadece server'da çalýþacak
            if (IsServer)
                StartCoroutine(FuseRoutine());
        }

        private IEnumerator FuseRoutine()
        {
            yield return new WaitForSeconds(fuseSeconds);

            if (damageSystem == null)
            {
                Debug.LogError("[BombController] damageSystem is NULL.");
                SafeDespawn();
                yield break;
            }

            // Explosion sadece server mantýðý
            damageSystem.Explode(originCell, range, mapLogicAdapter);

            SafeDespawn();
        }

        private void SafeDespawn()
        {
            BombRegistry.Unregister(originCell);

            var netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn(true);
            else
                Destroy(gameObject);
        }

        public override void OnNetworkDespawn()
        {
            // Güvenlik: her koþulda unregister
            BombRegistry.Unregister(originCell);
            base.OnNetworkDespawn();
        }
    }
}
