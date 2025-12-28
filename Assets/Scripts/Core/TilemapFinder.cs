using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapFinder
{
    public static Tilemap Find(string nameOrContains)
    {
        if (string.IsNullOrWhiteSpace(nameOrContains)) return null;

        var all = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);

        // 1) exact match
        foreach (var tm in all)
        {
            if (tm == null) continue;
            if (tm.gameObject.name.Equals(nameOrContains, StringComparison.OrdinalIgnoreCase))
                return tm;
        }

        // 2) contains match
        foreach (var tm in all)
        {
            if (tm == null) continue;
            if (tm.gameObject.name.IndexOf(nameOrContains, StringComparison.OrdinalIgnoreCase) >= 0)
                return tm;
        }

        return null;
    }
}
