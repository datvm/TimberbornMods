namespace SkyblightWeather.Services;

[BindSingleton]
public class BlightApplier(
    IDayNightCycle dayNightCycle,
    IWaterService waterService,
    IThreadSafeWaterMap waterMap,
    MapIndexService mapIndex
) : ITickableSingleton, ILoadableSingleton
{
    int verticalStride;

    int tickAccumulated;
    int ticksToAddOnePercent;

    int ticksPerHour;

    public void Load()
    {
        verticalStride = mapIndex.VerticalStride;
        ticksPerHour = dayNightCycle.HoursToTicks(1);
    }

    public void Start(float strength)
    {
        tickAccumulated = 0;
        ticksToAddOnePercent = strength > 0 ? Mathf.RoundToInt(ticksPerHour / strength / 100) : 0;
    }
    public void Stop() => Start(0);

    public void Tick()
    {
        if (ticksToAddOnePercent == 0) { return; }

        tickAccumulated++;
        if (tickAccumulated < ticksToAddOnePercent) { return; }

        tickAccumulated = 0;

        var columns = waterMap.WaterColumns;
        var columnCounts = waterMap.ColumnCounts;

        foreach (var i in mapIndex.Indices2D)
        {
            var cc = columnCounts[i];

            for (int k = 0; k < cc; k++)
            {
                var column = columns[k * verticalStride + i];
                if (column.Contamination >= 1f || column.WaterDepth == 0) { continue; }

                // Goal: add 1% contamination: 40% contamination becomes 41% contamination.

                var cleanWaterRemoving = column.WaterDepth * .01f;
                var availableCleanWater = column.WaterDepth * (1f - column.Contamination);
                if (cleanWaterRemoving > availableCleanWater)
                {
                    cleanWaterRemoving = availableCleanWater;
                }

                var coord = mapIndex.IndexToCoordinates(i, column.Floor);
                waterService.RemoveCleanWater(coord, cleanWaterRemoving);
                waterService.AddContaminatedWater(coord, cleanWaterRemoving);
            }
        }
    }

}
