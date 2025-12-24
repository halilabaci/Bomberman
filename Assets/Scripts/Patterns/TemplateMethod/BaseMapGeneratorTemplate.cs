using UnityEngine;
using DPBomberman.Patterns.Factory;

public abstract class BaseMapGeneratorTemplate : MonoBehaviour
{
    // Factory'yi bir kez alıp adımlarda kullanacağız
    protected IWallTileFactory currentFactory;

    // ✅ TEMPLATE METHOD: Akış sabit
    public void GenerateMap(IWallTileFactory factory = null)
    {
        currentFactory = factory;

        ClearMap();
        PlaceOuterWalls();
        PlaceInnerWalls();
        PlacePlayers();
        PlaceEnemies();
        PlacePowerUps();

        AfterGenerate();
    }

    // Adımlar (override edilebilir)
    protected abstract void ClearMap();
    protected abstract void PlaceOuterWalls();
    protected abstract void PlaceInnerWalls();

    protected virtual void PlacePlayers() { }
    protected virtual void PlaceEnemies() { }
    protected virtual void PlacePowerUps() { }

    // En sonda kamera vb.
    protected virtual void AfterGenerate() { }
}
