namespace TerraceFarm.Services;

[BindSingleton]
public class TerraceService(
    IBlockService blockService,
    DefaultEntityTracker<TerraceComponent> terraceTracker,
    DefaultEntityTracker<Growable> growableTracker
) : ILoadableSingleton
{
    readonly Dictionary<Vector3Int, TerraceCropPair> activePairs = [];

    public void Load()
    {
        terraceTracker.OnEntityRegistered += OnEntityChanged;
        terraceTracker.OnEntityUnregistered += OnEntityChanged;
        growableTracker.OnEntityRegistered += OnEntityChanged;
        growableTracker.OnEntityUnregistered += OnEntityChanged;
    }

    /// <summary>
    /// Terrace finished-state flip: visual pairing is unchanged, but bonus eligibility may change.
    /// </summary>
    public void OnTerraceFinishedStateChanged(TerraceComponent comp)
    {
        ReconsiderAround(comp.Coordinates);
    }

    void OnEntityChanged(BaseComponent obj)
    {
        var coords = obj.GetComponent<BlockObject>().Coordinates;
        UpdatePairAt(coords);
        ReconsiderAround(coords);
    }

    void UpdatePairAt(Vector3Int coords)
    {
        var hasRecord = activePairs.TryGetValue(coords, out var record);
        var pair = FindPairAt(coords);

        if (pair is { } p)
        {
            if (hasRecord && record.Terrace == p.Terrace && record.Crop == p.Crop)
            {
                return;
            }

            if (hasRecord)
            {
                record.Terrace.DetachCrop();
            }

            activePairs[coords] = p;
            // Rotation / UI: attach immediately regardless of finished state.
            p.Terrace.AttachCrop(p.Crop);
            return;
        }

        if (!hasRecord)
        {
            return;
        }

        record.Terrace.DetachCrop();
        activePairs.Remove(coords);
    }

    void ReconsiderAround(Vector3Int coords)
    {
        foreach (var n in Deltas.Neighbors8AndSelfVector3Int)
        {
            ReconsiderBonus(coords + n);
        }
    }

    void ReconsiderBonus(Vector3Int coords)
    {
        if (!activePairs.TryGetValue(coords, out var pair))
        {
            return;
        }

        var self = pair.Terrace;
        // Bonus only for finished terrace + paired crop.
        if (!self.ContributesToBonus)
        {
            self.NeighborPairs = 0;
            return;
        }

        var counter = 1; // include self
        foreach (var n in Deltas.Neighbors8Vector3IntOrdered)
        {
            if (activePairs.TryGetValue(coords + n, out var neighborPair)
                && neighborPair.Terrace.ContributesToBonus)
            {
                counter++;
            }
        }

        self.NeighborPairs = counter;
    }

    TerraceCropPair? FindPairAt(Vector3Int v)
    {
        if (FindAt<TerraceComponent>(v) is not { } t)
        {
            return null;
        }

        return FindAt<Growable>(v) is { } g ? new(t, g) : null;
    }

    public Growable? FindCropAt(Vector3Int v) => FindAt<Growable>(v);

    T? FindAt<T>(Vector3Int v) where T : BaseComponent
    {
        var comp = blockService.GetFirstObjectWithComponentAt<T>(v);
        return comp ? comp : null;
    }
}

public readonly record struct TerraceCropPair(TerraceComponent Terrace, Growable Crop);
