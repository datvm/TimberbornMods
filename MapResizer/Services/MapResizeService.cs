using System.Threading.Tasks;

namespace MapResizer.Services;

public class MapResizeService(
    MapSize mapSize,
    GameSaver gameSaver,
    SettlementNameService settlementNameService,
    MapIndexService mapIndexService,
    TerrainMapResizeService terrainMapResizeService,
    ColumnTerrainMapResizeService columnTerrainMapResizeService,
    SoilMapResizeService soilMapResizeService,
    WaterMapResizeService waterMapResizeService
)
{
    public static bool SkipFullTick { get; private set; }

    public async Task PerformResizeAsync(ResizeValues size, EnlargeStrategy enlargeStrategy)
    {
        SkipFullTick = true;

        var oldData = RetainOldMapSize();

        ResizeMapSize(size);
        ResizeMapIndexService();

        terrainMapResizeService.ResizeTerrainMap(oldData, enlargeStrategy);
        columnTerrainMapResizeService.Resize();
        soilMapResizeService.Resize();
        waterMapResizeService.Resize(oldData);

        await SaveGameAsync();

        SkipFullTick = false;
    }

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
        mapSize.TerrainSize = new(size.X, size.Y, size.Z1);
        mapSize.TotalSize = new(size.X, size.Y, size.Z1 + size.Z2);
    }

    void ResizeMapIndexService()
    {
        mapIndexService.Load();
    }

    async Task SaveGameAsync()
    {
        TaskCompletionSource<bool> tcs = new();

        gameSaver.QueueSave(
            new(settlementNameService.SettlementName, "MapResized"),
            () =>
            {
                Debug.Log("Resized map saved.");
                tcs.TrySetResult(true);
            });

        await tcs.Task;
    }

}

public enum EnlargeStrategy
{
    Copy,
    Mirror,
}

public readonly record struct ResizeData(MapSize MapSize, MapIndexService MapIndexService);