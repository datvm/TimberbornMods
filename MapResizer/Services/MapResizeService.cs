

namespace MapResizer.Services;

public class MapResizeService(
    MapSize mapSize,
    Ticker ticker,
    ISaverService gameSaver,
    MapIndexService mapIndexService,
    TerrainMapResizeService terrainMapResizeService,
    ColumnTerrainMapResizeService columnTerrainMapResizeService,
    SoilMapResizeService soilMapResizeService,
    WaterMapResizeService waterMapResizeService,
    PlantingMapResizer plantingMapResizer,
    TickOnlyArrayService tickOnlyArrayService
)
{
    public static bool PerformingResize { get; private set; }

    public async Task<ISaveReference> PerformResizeAsync(ResizeValues resizeValues)
    {
        ticker.FinishFullTick();
        PerformingResize = true;

        var oldData = RetainOldMapSize();
        tickOnlyArrayService._isLoadPhase = true;

        ResizeMapSize(resizeValues);
        ResizeMapIndexService();

        terrainMapResizeService.Resize(oldData, resizeValues.EnlargeStrategy);
        columnTerrainMapResizeService.Resize();
        soilMapResizeService.Resize();
        waterMapResizeService.Resize();
        plantingMapResizer.Resize();

        var saveRef = await SaveGameAsync();

        tickOnlyArrayService._isLoadPhase = false;
        PerformingResize = false;

        return saveRef;
    }

    public void LoadGame(ISaveReference saveReference) => gameSaver.Load(saveReference);

    ResizeData RetainOldMapSize()
    {
        var oldMapSize = new MapSize(null!)
        {
            TerrainSize = mapSize.TerrainSize,
            TotalSize = mapSize.TotalSize
        };

        var oldMapIndexService = new MapIndexService(oldMapSize);
        oldMapIndexService.Load();

        return new(oldMapSize, oldMapIndexService);
    }

    void ResizeMapSize(in ResizeValues size)
    {
        mapSize.TerrainSize = size.TerrainSize;
        mapSize.TotalSize = size.TotalSize;
    }

    void ResizeMapIndexService()
    {
        mapIndexService.Load();
    }

    async Task<ISaveReference> SaveGameAsync() => await gameSaver.SaveAsync();

}

public enum EnlargeStrategy
{
    Copy,
    Mirror,
}

public readonly record struct ResizeData(MapSize MapSize, MapIndexService MapIndexService);