using System.Collections.Generic;
using UnityEngine;

namespace DPBomberman.Controllers
{
    public static class BombRegistry
    {
        private static readonly Dictionary<Vector3Int, int> active = new();

        public static bool Has(Vector3Int cell)
        {
            return active.TryGetValue(cell, out int c) && c > 0;
        }

        public static void Register(Vector3Int cell)
        {
            if (active.TryGetValue(cell, out int c)) active[cell] = c + 1;
            else active[cell] = 1;
        }

        public static void Unregister(Vector3Int cell)
        {
            if (!active.TryGetValue(cell, out int c)) return;
            c -= 1;
            if (c <= 0) active.Remove(cell);
            else active[cell] = c;
        }

        public static void ClearAll() => active.Clear();
    }
}
