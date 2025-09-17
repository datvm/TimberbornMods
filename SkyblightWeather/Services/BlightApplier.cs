namespace SkyblightWeather.Services;

public class BlightApplier(
    SkyblightWeatherType skyblight,
    BadrainWeather badrain,
    SkyblightWeatherSettings s,
    IWaterService waterService,
    IThreadSafeWaterMap waterMap,
    MapIndexService mapIndex
) : ITickableSingleton, ILoadableSingleton
{
    int verticalStride;

    public void Load()
    {
        var size = mapIndex.TerrainSize;
        verticalStride = mapIndex.VerticalStride;
    }

    public void Tick()
    {
        if (!skyblight.Active && !badrain.Active) { return; }

        var str = s.SkyblightStrength.Value / 100f;
        if (str <= 0f) { return; }

        var columns = waterMap.WaterColumns;
        var columnCounts = waterMap.ColumnCounts;

        foreach (var i in mapIndex.Indices2D)
        {
            var cc = columnCounts[i];

            for (int k = 0; k < cc; k++)
            {
                var column = columns[k * verticalStride + i];
                if (column.Contamination >= 1f || column.WaterDepth == 0) { continue; }

                var removing = Mathf.Min(column.WaterDepth, column.WaterDepth * str);
                var coord = mapIndex.IndexToCoordinates(i, column.Floor);
                waterService.RemoveCleanWater(coord, removing);
                waterService.AddContaminatedWater(coord, removing);
            }
        }
    }

}
