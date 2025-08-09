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
    int mx, my, verticalStride;

    public void Load()
    {
        var size = mapIndex.TerrainSize;
        mx = size.x;
        my = size.y;
        verticalStride = mapIndex.VerticalStride;
    }

    public void Tick()
    {
        if (!skyblight.Active && !badrain.Active) { return; }

        var str = s.SkyblightStrength.Value / 100f;
        if (str <= 0f) { return; }

        var columns = waterMap.WaterColumns;
        var columnCounts = waterMap.ColumnCounts;
        var i = mapIndex.StartingIndex;
        for (int y = 0; y < my; y++)
        {
            for (int x = 0; x < mx; x++)
            {
                var cc = columnCounts[i];

                for (int k = 0; k < cc; k++)
                {
                    var column = columns[k * verticalStride + i];
                    if (column.Contamination >= 1f || column.WaterDepth == 0) { continue; }

                    var removing = Mathf.Min(column.WaterDepth, column.WaterDepth * str);
                    var coord = new Vector3Int(x,y, column.Floor);
                    waterService.RemoveCleanWater(coord, removing);
                    waterService.AddContaminatedWater(coord, removing);
                }
                i++;
            }

            i += 2;
        }
    }

}
