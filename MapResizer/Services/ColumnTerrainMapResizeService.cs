namespace MapResizer.Services;

public class ColumnTerrainMapResizeService(
    ColumnTerrainMap columnTerrainMap,
    IThreadSafeColumnTerrainMap iThreadSafeColumnTerrainMap,
    ITerrainService iTerrainService
)
{

    readonly ThreadSafeColumnTerrainMap threadSafeColumnTerrainMap = (ThreadSafeColumnTerrainMap)iThreadSafeColumnTerrainMap;
    readonly TerrainService terrainService = (TerrainService)iTerrainService;

    public void Resize()
    {
        columnTerrainMap.MaxColumnCount = 1;
        columnTerrainMap.Load();
        threadSafeColumnTerrainMap.Load();

        ResizeTerrainService();
    }

    void ResizeTerrainService()
    {
        terrainService.Load();
        terrainService.CalculateMinAndMaxHeight();
    }

}
