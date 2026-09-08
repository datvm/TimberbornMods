namespace MapTransformer.Components;

[MultiBind(typeof(IMapResizeComponent), Contexts = BindAttributeContext.NonMenu)]
class ColumnTerrainTransformComponent(
    ColumnTerrainMap columnTerrainMap,
    ThreadSafeColumnTerrainMap threadSafeColumnTerrainMap,
    TerrainService terrainService,
    MapIndexService mapIndexService
) : IMapResizeComponent
{
    public int Order => 20;

    public void Transform(in MapTransformContext ctx)
    {
        var vs = mapIndexService.VerticalStride;
        columnTerrainMap.MaxColumnCount = 1;
        columnTerrainMap._verticalStride = vs;
        columnTerrainMap._terrainColumns = new TerrainColumn[vs];
        columnTerrainMap.ColumnCount = new byte[vs];
        columnTerrainMap.LoadColumns();

        threadSafeColumnTerrainMap._verticalStride = vs;
        threadSafeColumnTerrainMap._columnCounts = new byte[mapIndexService.MaxIndex];
        threadSafeColumnTerrainMap._terrainColumns = new ReadOnlyTerrainColumn[vs * Math.Max(1, columnTerrainMap.MaxColumnCount)];
        threadSafeColumnTerrainMap.MaxColumnCount = columnTerrainMap.MaxColumnCount;
        columnTerrainMap.CopyTerrainColumnsData(
            threadSafeColumnTerrainMap._terrainColumns,
            threadSafeColumnTerrainMap._columnCounts,
            threadSafeColumnTerrainMap.MaxColumnCount);

        terrainService.Load();
        terrainService.CalculateMinAndMaxHeight();
    }
}
