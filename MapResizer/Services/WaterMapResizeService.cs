namespace MapResizer.Services;

public class WaterMapResizeService(
    WaterMap waterMap,
    WaterEvaporationMap waterEvaporationMap,
    IThreadSafeWaterMap iThreadSafeWaterMap,
    MapIndexService mapIndexService
)
{

    readonly ThreadSafeWaterMap threadSafeWaterMap = (ThreadSafeWaterMap)iThreadSafeWaterMap;

    public void Resize(in ResizeData oldData)
    {
        ResizeWaterMap(oldData);
        ResizeThreadSafeWaterMap();
        ResizeWaterEvaporationMap();
    }

    void ResizeWaterMap(in ResizeData oldData)
    {
        var (oldMapSize, oldMapIndexService) = oldData;

        var oldColumnCounts = waterMap.ColumnCount;
        var oldWaterColumns = waterMap._waterColumns;
        var oldOutflows = waterMap._outflows;

        var oldVerticalStride = oldMapIndexService.VerticalStride;
        var verticalStride = mapIndexService.VerticalStride;

        waterMap.MaxColumnCount = 1;
        waterMap._isInitialized = false;
        waterMap.Load();

        var oldSize = oldMapSize.TerrainSize;
        var newSize = mapIndexService.TerrainSize;

        var sx = Math.Min(oldSize.x, newSize.x);
        var sy = Math.Min(oldSize.y, newSize.y);

        var newColumnCounts = waterMap.ColumnCount;
        var newWaterColumns = waterMap._waterColumns;
        var newOutflows = waterMap._outflows;

        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                var oldIndex = oldMapIndexService.CellToIndex(new(x, y));
                var newIndex = mapIndexService.CellToIndex(new(x, y));

                var columnCount = Math.Min(
                    oldColumnCounts[oldIndex],
                    newColumnCounts[newIndex]
                );

                for (int z = 0; z < columnCount; z++)
                {
                    var oldIndex3D = oldIndex + z * oldVerticalStride;
                    var newIndex3D = newIndex + z * verticalStride;

                    newWaterColumns[newIndex3D] = oldWaterColumns[oldIndex3D];
                    newOutflows[newIndex3D] = oldOutflows[oldIndex3D];
                }
            }
        }
    }

    void ResizeWaterEvaporationMap()
    {
        waterEvaporationMap.OnMaxColumnCountChanged(waterMap, waterMap.MaxColumnCount);
    }

    void ResizeThreadSafeWaterMap()
    {
        threadSafeWaterMap.Load();

        threadSafeWaterMap._waterColumns = [.. waterMap._waterColumns];
        threadSafeWaterMap._columnCount = [.. waterMap.ColumnCount];
    }

}
