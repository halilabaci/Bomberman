using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Controllers
{
    public class ExplosionAreaTracker : MonoBehaviour
    {
        [Header("Explosion Lifetime")]
        public float activeSeconds = 0.35f;

        [Header("Refs")]
        public Tilemap groundTilemap;

        // Ayný hücre ayný anda birden fazla patlama tarafýndan aktif edilebilir.
        // Bu yüzden "kaç patlama aktif etti?" sayaç tutuyoruz.
        private readonly Dictionary<Vector3Int, int> activeCounts = new();

        public bool IsCellDangerous(Vector3Int cell) => activeCounts.TryGetValue(cell, out int c) && c > 0;

        public void ActivateCells(IEnumerable<Vector3Int> cells)
        {
            StartCoroutine(ActivateRoutine(cells));
        }

        private IEnumerator ActivateRoutine(IEnumerable<Vector3Int> cells)
        {
            // +1 sayaç
            foreach (var c in cells)
            {
                if (activeCounts.TryGetValue(c, out int count))
                    activeCounts[c] = count + 1;
                else
                    activeCounts[c] = 1;
            }

            yield return new WaitForSeconds(activeSeconds);

            // -1 sayaç
            foreach (var c in cells)
            {
                if (!activeCounts.TryGetValue(c, out int count))
                    continue;

                count -= 1;
                if (count <= 0)
                    activeCounts.Remove(c);
                else
                    activeCounts[c] = count;
            }
        }
    }
}
