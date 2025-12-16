using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Harita Ayarlarý")]
    public int width = 20;
    public int height = 15;
    public int seed = 123;

    [Header("Görseller (Tile'lar)")]
    public TileBase groundTile;
    public TileBase solidTile;
    public TileBase breakableTile;

    [Header("Unity Baðlantýlarý")]
    public Tilemap groundTilemap;
    public Tilemap solidTilemap;
    public Tilemap breakableTilemap;

    [Range(0, 1)]
    public float breakableDensity = 0.5f;

    void Start()
    {
        GenerateMap();
    }

    // 4 köþe spawn bölgeleri: köþe hücresi + 2 komþusu
    bool IsSpawnZone(int x, int y)
    {
        bool bottomLeft = (x == 1 && y == 1) || (x == 1 && y == 2) || (x == 2 && y == 1);
        bool bottomRight = (x == width - 2 && y == 1) || (x == width - 2 && y == 2) || (x == width - 3 && y == 1);
        bool topLeft = (x == 1 && y == height - 2) || (x == 1 && y == height - 3) || (x == 2 && y == height - 2);
        bool topRight = (x == width - 2 && y == height - 2) || (x == width - 2 && y == height - 3) || (x == width - 3 && y == height - 2);

        return bottomLeft || bottomRight || topLeft || topRight;
    }

    [ContextMenu("Haritayý Oluþtur")]
    public void GenerateMap()
    {
        if (!groundTilemap || !solidTilemap || !breakableTilemap)
        {
            Debug.LogError("Tilemap baðlantýlarý eksik! (ground/solid/breakable)");
            return;
        }
        if (!groundTile || !solidTile || !breakableTile)
        {
            Debug.LogError("Tile baðlantýlarý eksik! (ground/solid/breakable tile)");
            return;
        }

        groundTilemap.ClearAllTiles();
        solidTilemap.ClearAllTiles();
        breakableTilemap.ClearAllTiles();

        System.Random rng = new System.Random(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                groundTilemap.SetTile(pos, groundTile);

                // Kenar duvarlarý
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    solidTilemap.SetTile(pos, solidTile);
                }
                // Ýç kolonlar
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    solidTilemap.SetTile(pos, solidTile);
                }
                else
                {
                    // Spawn bölgeleri boþ kalsýn
                    if (IsSpawnZone(x, y))
                        continue;

                    if (rng.NextDouble() < breakableDensity)
                        breakableTilemap.SetTile(pos, breakableTile);
                }
            }
        }

        AdjustCamera();
    }

    void AdjustCamera()
    {
        if (Camera.main == null) return;
        Camera.main.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10f);
        Camera.main.orthographicSize = height / 2f + 1f;
    }
}
