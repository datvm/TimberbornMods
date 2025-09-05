namespace EarthquakeWeather.Services;

public class EarthquakeAreaService(
    MapSize mapSize,
    ISingletonLoader loader,
    EarthquakeRegistry earthquakeRegistry
) : ILoadableSingleton, ISaveableSingleton
{
    const int AreaSize = 10;

    static readonly SingletonKey SaveKey = new(nameof(EarthquakeAreaService));
    static readonly ListKey<Vector2Int> ScheduleAreaCenters = new("ScheduleAreaCenters");

    readonly LinkedList<EarthquakeArea> scheduledAreas = [];
    public IEnumerable<EarthquakeArea> ScheduledAreas => scheduledAreas;

    public void Load()
    {
        LoadSavedData();
    }

    public ImmutableArray<EarthquakeAreaBuilding> GetBuildingsInArea(EarthquakeArea area)
    {
        Dictionary<EarthquakeComponent, float> buildings = [];

        foreach (var tile in area.Tiles)
        {
            var tileBuildings = earthquakeRegistry.GetBuildingsAt(tile.Cell);
            var distance = tile.Distance;

            foreach (var b in tileBuildings)
            {
                if (!buildings.TryGetValue(b, out var existingDist) || distance < existingDist)
                {
                    buildings[b] = distance;
                }
            }
        }

        return [.. buildings
            .Select(kv => new EarthquakeAreaBuilding(kv.Key, kv.Value))
            .OrderBy(q => q.Distance)];
    }

    public EarthquakeArea PopNextArea()
    {
        if (scheduledAreas.Count > 0)
        {
            var area = scheduledAreas.First.Value;
            scheduledAreas.RemoveFirst();
            return area;
        }
        else
        {
            return GenerateAnArea();
        }
    }

    public EarthquakeArea ScheduleAnArea()
    {
        var area = GenerateAnArea();
        scheduledAreas.AddLast(area);

        return area;
    }

    public EarthquakeArea GenerateAnArea()
    {
        var building = PickRandomBuilding();
        return GetArea(building ? building.CoordiantesXY : Vector2Int.zero);
    }

    EarthquakeComponent? PickRandomBuilding()
    {
        var buildings = earthquakeRegistry.Buildings.ToArray();
        if (buildings.Length == 0) { return null; }

        var index = UnityEngine.Random.RandomRangeInt(0, buildings.Length);
        return buildings[index];
    }

    EarthquakeArea GetArea(Vector2Int center)
        => new(center, AreaSize, [.. GetAreaCells(center, AreaSize)]);

    IEnumerable<EarthquakeAreaCell> GetAreaCells(Vector2Int center, int r)
    {
        if (r <= 0) { yield break; }

        var r2 = r * r;
        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                var c = new Vector2Int(center.x + dx, center.y + dy);
                if (!mapSize.ContainsInTerrain(c)) { continue; } // outside the map

                int d2 = dx * dx + dy * dy;
                if (d2 > r2) { continue; } // outside the circle

                var dist = Mathf.Sqrt(d2);
                var normalized = dist / r;

                yield return new(c, normalized);
            }
        }
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(ScheduleAreaCenters))
        {
            var centers = s.Get(ScheduleAreaCenters);
            scheduledAreas.Clear();

            scheduledAreas.AddRange(centers.Select(GetArea));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(ScheduleAreaCenters, [.. scheduledAreas.Select(q => q.Center)]);
    }
}

public readonly record struct EarthquakeArea(Vector2Int Center, int Size, ImmutableArray<EarthquakeAreaCell> Tiles);
public readonly record struct EarthquakeAreaCell(Vector2Int Cell, float Distance);
public readonly record struct EarthquakeAreaBuilding(EarthquakeComponent EarthquakeComponent, float Distance);