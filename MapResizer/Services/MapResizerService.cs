

namespace MapResizer.Services;

public class MapResizerService(
    MapSize mapSize,
    GameSaver gameSaver,
    SettlementNameService settlementNameService,
    MapIndexService mapIndexService,
    TerrainMap terrainMap,
    ITerrainService terrainService,
    IThreadSafeColumnTerrainMap iThreadSafeColumnTerrainMap,
    SoilContaminationSimulator soilContaminationSimulator,
    SoilMoistureSimulator soilMoistureSimulator,
    WaterMap waterMap
) : ILoadableSingleton
{
    readonly ThreadSafeColumnTerrainMap threadSafeColumnTerrainMap = (ThreadSafeColumnTerrainMap)iThreadSafeColumnTerrainMap;

    public void Load()
    {
        Debug.Log("Map Terrain size: " + mapSize.TerrainSize);
        Debug.Log("Map Total size: " + mapSize.TotalSize);
    }

    public void PerformResize(ResizeValues size)
    {
        var oldTerrainSize = mapSize.TerrainSize;

        var newTerrainSize = mapSize.TerrainSize = new(size.X, size.Y, size.Z1);
        mapSize.TotalSize = new(size.X, size.Y, size.Z1 + size.Z2);

        mapIndexService.Load(); // Reload the size
        ResizeTerrainVoxels(oldTerrainSize, newTerrainSize);

        threadSafeColumnTerrainMap.Tick();

        ResizeMaps();

        SaveGame();
    }

    void ResizeTerrainVoxels(in Vector3Int oldSize, in Vector3Int newSize)
    {
        var maxX = newSize.x;
        var maxY = newSize.y;
        var maxZ = newSize.z;

        for (int x = 0; x < oldSize.x; x++)
        {
            for (int y = 0; y < oldSize.y; y++)
            {
                for (int z = 0; z < oldSize.z; z++)
                {
                    if (x > maxX || y > maxY || z > maxZ)
                    {
                        terrainService.UnsetTerrain(new(x, y, z));
                    }
                }
            }
        }

        Array.Resize(ref terrainMap._terrainVoxels, mapIndexService.MaxSize3D);
    }

    void ResizeMaps()
    {
        var verticalStride = mapIndexService.VerticalStride;
        var max = threadSafeColumnTerrainMap.MaxColumnCount * verticalStride;
        Array.Resize(ref soilContaminationSimulator._contaminationLevels, max);
        Array.Resize(ref soilContaminationSimulator._contaminationCandidates, max);

        Array.Resize(ref soilMoistureSimulator._moistureLevels, max);

        
        Array.Resize(ref waterMap._waterColumns, verticalStride);
        Array.Resize(ref waterMap._outflows, verticalStride);

        waterMap.ColumnCount = [.. waterMap.ColumnCount.Take(verticalStride)];
    }

    void SaveGame()
    {
        gameSaver.QueueSave(
            new(settlementNameService.SettlementName, "MapResized"),
            static () => { Debug.Log("Resized map saved."); });
    }

}
