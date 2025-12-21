using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Controllers;
using DPBomberman.Patterns.Strategy;


// EnemyController başka bir namespace'teyse burayı ona göre düzeltiriz.
// Şimdilik gerek yok; GetComponent ile alıyoruz.

public class EnemySpawnerRandom : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Tilemaps (Inspector preferred)")]
    public Tilemap ground;
    public Tilemap wallsSolid;
    public Tilemap wallsBreakable;
    public Tilemap wallsHard;
    public Tilemap decorations; // ✅ GameObject.Find yerine referans (opsiyonel)

    [Header("Spawn")]
    public int enemyCount = 2;
    public int maxAttemptsPerEnemy = 300;

    [Header("Player")]
    public Transform player;
    public int minManhattanDistanceFromPlayer = 4;

    [Header("AI (Strategy)")]
    public EnemyStrategyType spawnStrategy = EnemyStrategyType.Random; // ✅ sadece seçim

    private IEnumerator Start()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("[EnemySpawnerRandom] enemyPrefab NULL");
            yield break;
        }

        // Inspector set edilmediyse otomatik bul (1 kez)
        if (!ground) ground = FindTilemapSafe("Ground");
        if (!wallsSolid) wallsSolid = FindTilemapSafe("Walls_Solid");
        if (!wallsBreakable) wallsBreakable = FindTilemapSafe("Walls_Breakable");
        if (!wallsHard) wallsHard = FindTilemapSafe("Walls_Hard");
        if (!decorations) decorations = FindTilemapSafe("Decorations"); // ✅ tek sefer bul

        // ✅ Loglar: asla exception fırlatmasın
        Debug.Log($"[EnemySpawnerRandom] ground={(ground ? ground.name : "NULL")} used={SafeUsedTilesCount(ground)}");
        Debug.Log($"[EnemySpawnerRandom] deco used={SafeUsedTilesCount(decorations)}");

        // Harita çizilene kadar bekle (max ~ 2sn)
        int safety = 120;
        while (ground != null && SafeUsedTilesCount(ground) == 0 && safety-- > 0)
            yield return null;

        if (ground == null)
        {
            Debug.LogError("[EnemySpawnerRandom] ground tilemap bulunamadı!");
            yield break;
        }

        int used = SafeUsedTilesCount(ground);
        Debug.Log($"[EnemySpawnerRandom] Ground ready. usedTiles={used}");

        if (used == 0)
        {
            Debug.LogError("[EnemySpawnerRandom] Ground usedTiles STILL 0. MapGenerator ground'a çizmiyor veya yanlış tilemap seçili!");
            yield break;
        }

        ground.CompressBounds();
        Debug.Log($"[EnemySpawnerRandom] Bounds={ground.cellBounds}");

        int spawned = 0;
        for (int i = 0; i < enemyCount; i++)
            if (TrySpawnOne()) spawned++;

        Debug.Log($"[EnemySpawnerRandom] Spawn result: {spawned}/{enemyCount}");
    }

    private bool TrySpawnOne()
    {
        if (ground == null) return false;

        var b = ground.cellBounds;

        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            int x = Random.Range(b.xMin, b.xMax);
            int y = Random.Range(b.yMin, b.yMax);
            Vector3Int cell = new Vector3Int(x, y, 0);

            // Tile kontrolü (ground yoksa zaten dönmüştük)
            if (!ground.HasTile(cell)) continue;

            // Duvarlar (destroy olmuş olabilir => null check otomatik)
            if (wallsSolid && wallsSolid.HasTile(cell)) continue;
            if (wallsBreakable && wallsBreakable.HasTile(cell)) continue;
            if (wallsHard && wallsHard.HasTile(cell)) continue;

            // Player'dan uzaklık
            if (player)
            {
                var pCell = ground.WorldToCell(player.position);
                int dist = Mathf.Abs(cell.x - pCell.x) + Mathf.Abs(cell.y - pCell.y);
                if (dist < minManhattanDistanceFromPlayer) continue;
            }

            Vector3 pos = ground.GetCellCenterWorld(cell);

            // ✅ Mantığı bozmadan: instantiate sonrası strategy set
            GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
            var ctrl = go.GetComponent<EnemyController>();
            if (ctrl != null)
            {
                ctrl.SetStrategy(spawnStrategy);
            }
            else
            {
                Debug.LogWarning("[EnemySpawnerRandom] Spawned enemy has no EnemyController component!");
            }

            return true;
        }

        return false;
    }

    // -------------------------
    // SAFE HELPERS (no errors)
    // -------------------------

    private static Tilemap FindTilemapSafe(string goName)
    {
        var go = GameObject.Find(goName);
        return go ? go.GetComponent<Tilemap>() : null;
    }

    private static int SafeUsedTilesCount(Tilemap tm)
    {
        // Unity'de destroyed object bazen "null" gibi davranır.
        if (tm == null) return 0;

        try
        {
            // Senin extension/metodun (GetUsedTilesCount) burada çağrılır.
            return tm.GetUsedTilesCount();
        }
        catch (MissingReferenceException)
        {
            // Destroy edilmiş referansa dokunulduysa -> error basma, 0 dön
            return 0;
        }
    }
}
