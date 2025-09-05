namespace EarthquakeWeather.Services;

public class EarthquakeRegistry
{

    readonly HashSet<EarthquakeComponent> comps = [];
    public IReadOnlyCollection<EarthquakeComponent> Buildings => comps;

    readonly Dictionary<Vector2Int, List<EarthquakeComponent>> buildingsByCoord = [];

    readonly HashSet<EarthquakeStockpileComponent> stockpiles = [];
    public IReadOnlyCollection<EarthquakeStockpileComponent> Stockpiles => stockpiles;

    public void Register(EarthquakeComponent comp)
    {
        comps.Add(comp);

        var tiles = comp.BlockObjectTiles;
        foreach (var tile in tiles)
        {
            var list = GetOrCreateList(tile);
            list.Add(comp);
        }
    }

    public void Unregister(EarthquakeComponent comp)
    {
        comps.Remove(comp);

        var tiles = comp.BlockObjectTiles;
        foreach (var tile in tiles)
        {
            var list = GetOrCreateList(tile);
            list.Remove(comp);
        }
    }

    public void Register(EarthquakeStockpileComponent comp) => stockpiles.Add(comp);
    public void Unregister(EarthquakeStockpileComponent comp) => stockpiles.Remove(comp);

    public IEnumerable<EarthquakeComponent> GetBuildingsAt(Vector2Int coord) 
        => GetOrCreateList(coord)
            .Where(q => q);

    public IEnumerable<EarthquakeComponent> GetBuildingsInArea(IEnumerable<Vector2Int> area) 
        => area
            .SelectMany(GetBuildingsAt)
            .Distinct();

    List<EarthquakeComponent> GetOrCreateList(Vector2Int coord)
    {
        if (!buildingsByCoord.TryGetValue(coord, out var list))
        {
            list = buildingsByCoord[coord] = [];
        }

        return list;
    }

}
