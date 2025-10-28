namespace ConveyorBelt.Services;

public class ConveyorBeltService(
    IBlockService blockService,
    EventBus eb,
    RollingHighlighter highlighter
) : ILoadableSingleton
{
    public static readonly Vector3Int Above = new(0, 0, 1);
    public static readonly Vector3Int Below = new(0, 0, -1);
    public static readonly Vector3Int[] BeltOrientationPrevious = [new(0, 1), new(1, 0), new(0, -1), new(-1, 0),];
    public static readonly Vector3Int[] BeltOrientationNext = [new(0, -1), new(-1, 0), new(0, 1), new(1, 0),];
    public static readonly Color HighlightColor = new(1f, 0.8f, 0.45f, 0.5f);

    readonly Dictionary<ConveyorBeltComponent, ConveyorBeltCluster> clusterOfBelts = [];
    readonly HashSet<ConveyorBeltCluster> clusters = [];

    ConveyorBeltCluster? highlightingCluster;

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnBuildingFinished(EnteredFinishedStateEvent e)
    {

    }

    [OnEvent]
    public void OnBuildingDemolished(ExitedFinishedStateEvent e)
    {

    }

    public void HighlighCluster(ConveyorBeltComponent belt)
    {
        highlightingCluster = clusterOfBelts[belt];
        highlighter.HighlightPrimary(
            highlightingCluster.Value.Belts.Where(q => q != belt),
            HighlightColor);
    }

    public void UnhighlightCluster()
    {
        highlightingCluster = null;
        highlighter.UnhighlightAllPrimary();
    }

    public void Register(ConveyorBeltComponent comp)
    {
        Unregister(comp);

        var conn = comp.Connection;
        var coord = conn.Coordinates;

        var prev = FindSatisfyingBelt(conn.PreviousCoords, b => b.Connection.NextCoords == coord);
        var next = FindSatisfyingBelt(conn.NextCoords, b => b.Connection.PreviousCoords == coord);

        ConveyorBeltCluster?
            prevCluster = prev ? clusterOfBelts[prev] : null,
            nextCluster = next ? clusterOfBelts[next] : null;
        ConnectClusters(prevCluster, nextCluster, comp);
    }

    public void Unregister(ConveyorBeltComponent comp)
    {
        if (clusterOfBelts.TryGetValue(comp, out var clusterOfBelt))
        {
            SplitCluster(clusterOfBelt, comp);
        }
    }

    public ConveyorBeltComponent? FindActiveBelt(Vector3Int coord)
    {
        var belt = blockService.GetObjectsWithComponentAt<ConveyorBeltComponent>(coord)
            .FirstOrDefault(q => q && q.Active);
        return belt ? belt : null;
    }

    public IEnumerable<Stockpile> GetAvailableSources(ConveyorBeltCluster cluster)
        => GetAvailableStockpiles(cluster, cluster.Source, cluster.InputCoordinates, () => cluster.OutputCoordinates);

    public IEnumerable<Stockpile> GetAvailableDestinations(ConveyorBeltCluster cluster)
        => GetAvailableStockpiles(cluster, cluster.Destination, cluster.OutputCoordinates, () => cluster.InputCoordinates);

    IEnumerable<Stockpile> GetAvailableStockpiles(ConveyorBeltCluster cluster, Stockpile? selecting, Vector3Int coords, Func<Vector3Int> additionalLiftCoords)
    {
        var stockpiles = GetStockpilesAt(coords);
        if (cluster.IsLift)
        {
            stockpiles = stockpiles.Concat(GetStockpilesAt(additionalLiftCoords()));
        }

        foreach (var sp in stockpiles)
        {
            if (selecting == sp || (cluster.Source != sp && cluster.End != sp))
            {
                yield return sp;
            }
        }
    }

    IEnumerable<Stockpile> GetStockpilesAt(Vector3Int coords)
        => blockService.GetObjectsWithComponentAt<Stockpile>(coords);

    void ConnectClusters(ConveyorBeltCluster? prev, ConveyorBeltCluster? next, ConveyorBeltComponent curr)
    {
        if (prev is not null)
        {
            DeleteCluster(prev.Value);
        }

        if (next is not null)
        {
            DeleteCluster(next.Value);
        }

        CreateCluster([.. prev?.Belts ?? [], curr, .. next?.Belts ?? []]);
    }

    void SplitCluster(ConveyorBeltCluster cluster, ConveyorBeltComponent removedBelt)
    {
        List<ConveyorBeltComponent> before = [], after = [];
        var found = false;

        foreach (var belt in cluster.Belts)
        {
            if (belt == removedBelt)
            {
                found = true;
                continue;
            }
            else if (found)
            {
                after.Add(belt);
            }
            else
            {
                before.Add(belt);
            }
        }

        if (!found)
        {
            throw new InvalidOperationException("The belt to be removed is not found in the cluster.");
        }

        DeleteCluster(cluster);
        CreateCluster(before);
        CreateCluster(after);
    }

    void DeleteCluster(ConveyorBeltCluster cluster)
    {
        foreach (var belt in cluster.Belts)
        {
            clusterOfBelts.Remove(belt);
        }

        clusters.Remove(cluster);

        if (highlightingCluster.HasValue && highlightingCluster.Value.Equals(cluster))
        {
            UnhighlightCluster();
        }
    }

    void CreateCluster(IReadOnlyList<ConveyorBeltComponent> belts)
    {
        if (belts.Count == 0) { return; }

        var cluster = new ConveyorBeltCluster(belts);
        clusters.Add(cluster);

        ConveyorBeltComponent? prev = null;
        foreach (var belt in belts)
        {
            clusterOfBelts[belt] = cluster;
            belt.SetCluster(cluster);

            if (prev is not null)
            {
                prev.Connection.NextBelt = belt;
                belt.Connection.PreviousBelt = prev;
            }

            prev = belt;
        }
    }

    ConveyorBeltComponent? FindSatisfyingBelt(Vector3Int coord, Func<ConveyorBeltComponent, bool> predicate)
    {
        var belt = FindActiveBelt(coord);
        return belt && predicate(belt) ? belt : null;
    }

}
