namespace WirelessCoil.Services;

public class WirelessCoilService(
    MechanicalGraphFactory mechanicalGraphFactory
) : ILoadableSingleton, IUnloadableSingleton, ITickableSingleton
{
    bool hasUninitializedGraph = false;

    public static WirelessCoilService? Instance { get; private set; }

    readonly HashSet<WirelessCoilComponent> wirelessCoils = [];
    public IReadOnlyCollection<WirelessCoilComponent> WirelessCoils => wirelessCoils;

    public void Register(WirelessCoilComponent comp)
    {
        if (!comp) { return; }

        wirelessCoils.Add(comp);
        OnNetworkChanged();
    }

    public void Unregister(WirelessCoilComponent comp)
    {
        wirelessCoils.Remove(comp);
        OnNetworkChanged();
    }

    void OnNetworkChanged()
    {
        ScanForConnections();
        ConnectGraphs();
    }

    public void ConnectGraphs()
    {
        foreach (var coil in wirelessCoils)
        {
            if (!coil)
            {
                hasUninitializedGraph = true;
                continue;
            }

            if (coil.ConnectedCoils.Count == 0) { continue; }

            var srcGraph = coil.MechanicalGraph;
            if (srcGraph is null)
            {
                hasUninitializedGraph = true;
                continue;
            }

            HashSet<MechanicalGraph> graphs = [srcGraph];
            foreach (var o in coil.ConnectedCoils)
            {
                if (!o)
                {
                    hasUninitializedGraph = true;
                    continue;
                }
                var graph = o.MechanicalGraph;

                if (graph is null)
                {
                    hasUninitializedGraph = true;
                    continue;
                }
                graphs.Add(graph);
            }

            if (graphs.Count < 2) { continue; }

            mechanicalGraphFactory.Join(graphs);
        }
    }

    void ScanForConnections()
    {
        var coils = wirelessCoils.ToArray();

        var connected = new List<WirelessCoilComponent>[coils.Length];
        for (int i = 0; i < coils.Length; i++)
        {
            connected[i] = [];
        }

        for (int i = 0; i < coils.Length; i++)
        {
            var c = coils[i];
            if (!c) { continue; }

            var curr = connected[i];
            var range = c.Range;
            var min = c.Coordinates - new Vector3Int(range, range, range);
            var max = c.Coordinates + new Vector3Int(range, range, range);

            for (int j = i + 1; j < coils.Length; j++)
            {
                var o = coils[j];
                if (!o) { continue; }
                var coor = o.Coordinates;

                if (coor.x < min.x || coor.y < min.y || coor.z < min.z) { continue; }
                if (coor.x > max.x || coor.y > max.y || coor.z > max.z) { continue; }

                curr.Add(o);
                connected[j].Add(c);
            }

            c.ConnectedCoils = curr;
        }
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }

    public void Tick()
    {
        if (!hasUninitializedGraph) { return; }

        hasUninitializedGraph = false;
        var coils = wirelessCoils.ToArray();

        // First remove all invalid ones
        foreach (var c in coils)
        {
            if (!c && c is not null)
            {
                wirelessCoils.Remove(c);
            }
        }

        // Reconnect coils and graphs
        ScanForConnections();
        ConnectGraphs();
    }
}
