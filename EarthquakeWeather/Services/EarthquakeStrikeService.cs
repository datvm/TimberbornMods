namespace EarthquakeWeather.Services;

public class EarthquakeStrikeService(
    Earthquake earthquake,
    EarthquakeAreaService earthquakeAreaService,
    IThreadSafeWaterMap waterMap,
    MapIndexService mapIndex,
    IWaterService waterService,
    CameraShakeService cameraShakeService,
    EarthquakeNotificationService earthquakeNotification
) : ILoadableSingleton
{
    int mx, my, verticalStride;

    public event Action<EarthquakeArea> OnEarthquakeAreaStrike = null!;

    public void Load()
    {
        var size = mapIndex.TerrainSize;
        mx = size.x;
        my = size.y;
        verticalStride = mapIndex.VerticalStride;

        earthquake.OnEarthquakeHit += Strike;
        OnEarthquakeAreaStrike += OnAreaStrike;
    }

    private void OnAreaStrike(EarthquakeArea area)
    {
        var dmg = earthquake.DamageStrength;
        var minDamage = dmg.x;
        var maxDamage = dmg.y;

        var buildings = earthquakeAreaService.GetBuildingsInArea(area);
        earthquakeNotification.SetLocation(area.Center);

        foreach (var (b, distance) in buildings)
        {
            var damage = (int)Mathf.Lerp(maxDamage, minDamage, distance);
            b.Damage(damage, earthquakeNotification);
        }
    }

    public void Strike()
    {
        ShakeCamera();
        FindAndDamageArea();
        SurgeWater();
    }

    void ShakeCamera() => cameraShakeService.Shake(3f, 3f);

    void FindAndDamageArea()
    {
        var area = earthquakeAreaService.PopNextArea();
        OnEarthquakeAreaStrike?.Invoke(area);
    }

    void SurgeWater()
    {
        var surge = earthquake.SurgeStrength;

        List<KeyValuePair<Vector3Int, Vector2>> addingWater = [];
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
                    var adding = column.WaterDepth * surge / 2;

                    var addingConta = adding * column.Contamination;
                    var addingClean  = adding - addingConta;

                    var coord = new Vector3Int(x, y, column.Floor);
                    addingWater.Add(new(coord, new(addingClean, addingConta)));
                }
                i++;
            }

            i += 2;
        }

        foreach (var (coord, adding) in addingWater)
        {
            waterService.AddCleanWater(coord, adding.x);
            waterService.AddContaminatedWater(coord, adding.y);
        }
    }

}
