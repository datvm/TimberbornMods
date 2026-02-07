namespace PlottingTool.Services;

public class PlotterService(
    ISingletonLoader loader,
    MapSize mapSize
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("LV.PlottingTool");
    static readonly ListKey<Vector2Int> BeaconsKey = new("Beacons");
    static readonly Color MidlineColor = Color.orangeRed;

    Beacon? prevBeacon;
    readonly Dictionary<Vector2Int, Beacon> beacons = [];
    readonly Dictionary<Vector2Int, HashSet<BeaconPair>> relatedBeaconLocations = [];

    public IReadOnlyCollection<Vector2Int> Beacons => beacons.Keys;
    bool disableMidliner = true;
    
    public void Load()
    {
        LoadSavedData();
        disableMidliner = false;        
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(BeaconsKey))
        {
            var locations = s.Get(BeaconsKey);
            foreach (var loc in locations)
            {
                AddBeacon(loc);
            }
        }
    }

    public void DisableHorizontalLines()
    {
        foreach (var b in beacons.Values)
        {
            b.DisableHorizontalLines();
        }
        foreach (var list in relatedBeaconLocations.Values)
        {
            foreach (var pair in list)
            {
                pair.Midline.DisableHorizontalLines();
            }
        }
    }

    public void ShowHorizontalLines(int level)
    {
        foreach (var b in beacons.Values)
        {
            b.ShowHorizontalLines(level);
        }

        foreach (var list in relatedBeaconLocations.Values)
        {
            foreach (var pair in list)
            {
                pair.Midline.ShowHorizontalLines(level);
            }
        }
    }

    public bool ToggleBeacon(Vector2Int location)
    {
        if (!AddBeacon(location))
        {
            RemoveBeacon(location);
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool AddBeacon(Vector2Int location)
    {
        if (beacons.ContainsKey(location) || HasRelatedBeaconAt(location)) { return false; }

        beacons.Add(location, PlotBeacon(location));
        return true;
    }

    public bool RemoveBeacon(Vector2Int location)
    {
        if (TryRemovingRelatedBeacons(location)) { return true; }

        if (!beacons.TryGetValue(location, out var b)) { return false; }

        RemoveMainBeacon(b);
        return true;
    }

    public void ClearAllBeacons()
    {
        foreach (var b in beacons.Values.ToArray())
        {
            RemoveMainBeacon(b);
        }
        beacons.Clear();
        relatedBeaconLocations.Clear();
    }

    void RemoveMainBeacon(Beacon b)
    {
        foreach (var pair in b.RelatedPairs.ToArray())
        {
            RemoveMidliner(pair);
        }

        beacons.Remove(b.Location);
        b.Dispose();
    }

    bool HasRelatedBeaconAt(Vector2Int location) => relatedBeaconLocations.TryGetValue(location, out var list) && list.Count > 0;

    bool TryRemovingRelatedBeacons(Vector2Int location)
    {
        if (!relatedBeaconLocations.TryGetValue(location, out var list) || list.Count == 0) { return false; }

        foreach (var item in list.ToArray())
        {
            RemoveMidliner(item);
        }
        relatedBeaconLocations.Remove(location);

        return true;
    }

    void RemoveMidliner(BeaconPair pair)
    {
        pair.Midline.Dispose();
        relatedBeaconLocations[pair.Midline.Location].Remove(pair);
        pair.Beacon1.RelatedPairs.Remove(pair);
        pair.Beacon2.RelatedPairs.Remove(pair);
    }

    Beacon PlotBeacon(Vector2Int location)
    {
        var size = mapSize.TotalSize;
        var result = new Beacon(location, size);

        if (!disableMidliner)
        {
            TryAddingMidliner(result, size);
        }

        return prevBeacon = result;
    }

    Vector2Int? TryGetMiddleCell(Vector2Int b1, Vector2Int b2)
        => ((b1.x + b2.x) % 2 == 0 && (b1.y + b2.y) % 2 == 0)
            ? (b1 + b2) / 2
            : null;

    void TryAddingMidliner(Beacon newBeacon, Vector3Int size)
    {
        if (prevBeacon?.Disposed != false) { return; }

        var midLocation = TryGetMiddleCell(prevBeacon.Location, newBeacon.Location);
        if (!midLocation.HasValue) { return; }

        var midBeacon = new Beacon(midLocation.Value, size, MidlineColor);
        var pair = new BeaconPair(prevBeacon, newBeacon, midBeacon);

        prevBeacon.RelatedPairs.Add(pair);
        newBeacon.RelatedPairs.Add(pair);
        midBeacon.RelatedPairs.Add(pair);

        if (!relatedBeaconLocations.TryGetValue(midLocation.Value, out var list))
        {
            relatedBeaconLocations[midLocation.Value] = list = [];
        }
        list.Add(pair);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(BeaconsKey, beacons.Keys);
    }
}

