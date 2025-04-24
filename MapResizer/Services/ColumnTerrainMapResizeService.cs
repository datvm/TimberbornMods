namespace MapResizer.Services;

public class ColumnTerrainMapResizeService(
    ColumnTerrainMap columnTerrainMap,
    IThreadSafeColumnTerrainMap iThreadSafeColumnTerrainMap
)
{

    readonly ThreadSafeColumnTerrainMap threadSafeColumnTerrainMap = (ThreadSafeColumnTerrainMap)iThreadSafeColumnTerrainMap;

    public void Resize()
    {
        columnTerrainMap.MaxColumnCount = 1;
        columnTerrainMap.Load();
        threadSafeColumnTerrainMap.Load();
    }

}
