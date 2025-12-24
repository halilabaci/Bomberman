using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models;
using DPBomberman.Patterns.Factory;

public class MapGenerator : BaseMapGeneratorTemplate
{
    [Header("Harita Ayarları")]
    public int width = 20;
    public int height = 15;
    public int seed = 123;

    [Header("Görseller (Tile'lar) - Fallback (Factory yoksa)")]
    public TileBase groundTile;
    public TileBase solidTile;
    public TileBase breakableTile;
    public TileBase hardTile;

    [Header("Unity Bağlantıları")]
    public Tilemap groundTilemap;
    public Tilemap solidTilemap;
    public Tilemap breakableTilemap;
    public Tilemap hardTilemap;

    [Range(0, 1)] public float breakableDensity = 0.5f;
    [Range(0, 1)] public float hardDensity = 0.15f; // breakable içinde hard oranı

    // Runtime
    private System.Random rng;

    // Cache tiles for current generation (factory ya da fallback)
    private TileBase gTile, sTile, bTile, hTile;

    private bool IsSpawnZone(int x, int y)
    {
        bool bottomLeft = (x == 1 && y == 1) || (x == 1 && y == 2) || (x == 2 && y == 1);
        bool bottomRight = (x == width - 2 && y == 1) || (x == width - 2 && y == 2) || (x == width - 3 && y == 1);
        bool topLeft = (x == 1 && y == height - 2) || (x == 1 && y == height - 3) || (x == 2 && y == height - 2);
        bool topRight = (x == width - 2 && y == height - 2) || (x == width - 2 && y == height - 3) || (x == width - 3 && y == height - 2);

        return bottomLeft || bottomRight || topLeft || topRight;
    }

    // ✅ 1) ClearMap
    protected override void ClearMap()
    {
        if (!groundTilemap || !solidTilemap || !breakableTilemap)
        {
            Debug.LogError("Tilemap bağlantıları eksik! (ground/solid/breakable)");
            return;
        }

        // Factory varsa tile'ları oradan al; yoksa fallback kullan
        gTile = currentFactory != null ? currentFactory.GetTile(WallType.Ground) : groundTile;
        sTile = currentFactory != null ? currentFactory.GetTile(WallType.Unbreakable) : solidTile;
        bTile = currentFactory != null ? currentFactory.GetTile(WallType.Breakable) : breakableTile;
        hTile = currentFactory != null ? currentFactory.GetTile(WallType.Hard) : hardTile;

        // Minimum tile kontrolü
        if (!gTile || !sTile || !bTile)
        {
            Debug.LogError(
                "Tile bağlantıları eksik! (ground/solid/breakable). " +
                "Factory kullanıyorsan ThemeFactorySO slotlarını doldur, yoksa Inspector tile'larını doldur."
            );
            return;
        }

        if (hardTilemap == null && hTile != null)
            Debug.LogWarning("[MapGenerator] Hard tile var ama hardTilemap atanmadı. Hard duvar basılmayacak.");

        groundTilemap.ClearAllTiles();
        solidTilemap.ClearAllTiles();
        breakableTilemap.ClearAllTiles();
        hardTilemap?.ClearAllTiles();

        rng = new System.Random(seed);

        // Ground'u burada basalım (senin önceki koddaki gibi)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                groundTilemap.SetTile(new Vector3Int(x, y, 0), gTile);
    }

    // ✅ 2) OuterWalls
    protected override void PlaceOuterWalls()
    {
        for (int x = 0; x < width; x++)
        {
            solidTilemap.SetTile(new Vector3Int(x, 0, 0), sTile);
            solidTilemap.SetTile(new Vector3Int(x, height - 1, 0), sTile);
        }
        for (int y = 0; y < height; y++)
        {
            solidTilemap.SetTile(new Vector3Int(0, y, 0), sTile);
            solidTilemap.SetTile(new Vector3Int(width - 1, y, 0), sTile);
        }
    }

    // ✅ 3) InnerWalls (senin mevcut mantığın)
    protected override void PlaceInnerWalls()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // İç kolonlar
                if (x % 2 == 0 && y % 2 == 0)
                {
                    solidTilemap.SetTile(pos, sTile);
                    continue;
                }

                // Spawn bölgeleri boş kalsın
                if (IsSpawnZone(x, y)) continue;

                // Breakable / Hard dağılımı
                if (rng.NextDouble() < breakableDensity)
                {
                    if (hardTilemap != null && hTile != null && rng.NextDouble() < hardDensity)
                        hardTilemap.SetTile(pos, hTile);
                    else
                        breakableTilemap.SetTile(pos, bTile);
                }
            }
        }
    }

    // ✅ 4-5-6 şimdilik boş bırakabiliriz (FAZ 4 rapor için yeter)
    protected override void PlacePlayers() { /* İstersen sonra spawn ekleriz */ }
    protected override void PlaceEnemies() { /* İstersen sonra spawn ekleriz */ }
    protected override void PlacePowerUps() { /* İstersen sonra dağıtırız */ }

    // ✅ Kamera vs.
    protected override void AfterGenerate()
    {
        AdjustCamera();
    }

    private void AdjustCamera()
    {
        if (Camera.main == null) return;
        Camera.main.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10f);
        Camera.main.orthographicSize = height / 2f + 1f;
    }

    // Inspector'dan test için
    [ContextMenu("Haritayı Oluştur")]
    private void GenerateFromContextMenu()
    {
        GenerateMap(null);
    }
}
