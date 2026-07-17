namespace PowerLines.Services;

[BindSingleton]
public class PowerLineGraphService(
    PowerLineConnectionService connectionService,
    MechanicalGraphFactory graphFactory,
    MechanicalGraphReorganizer graphReorganizer
) : ILoadableSingleton, IUnloadableSingleton
{
    public static PowerLineGraphService? Instance { get; private set; }

    public void Load()
    {
        Instance = this;
        connectionService.ConnectionAdded += OnConnectionAdded;
        connectionService.ConnectionRemoved += OnConnectionRemoved;
    }

    public void Unload()
    {
        connectionService.ConnectionAdded -= OnConnectionAdded;
        connectionService.ConnectionRemoved -= OnConnectionRemoved;
        Instance = null;
    }

    public IReadOnlyList<PowerLineComponent> GetConnectedPowerLines(PowerLineComponent powerLine)
        => [.. connectionService.GetConnectedPowerLines(powerLine)];

    public IEnumerable<MechanicalNode> GetPowerLineLinkedNodes(MechanicalNode node)
    {
        var powerLine = node.GetComponent<PowerLineComponent>();
        if (!powerLine) { yield break; }

        foreach (var other in connectionService.GetConnectedPowerLines(powerLine))
        {
            if (!other || other.IsDeleted || !other.IsFinished) { continue; }

            var otherNode = other.GetComponent<MechanicalNode>();
            if (otherNode) { yield return otherNode; }
        }
    }

    public void OnNodeAddedToGraph(MechanicalNode node)
    {
        var powerLine = node.GetComponent<PowerLineComponent>();
        if (!powerLine || !powerLine.IsFinished) { return; }

        JoinWithPowerLinePartners(node, powerLine);
    }

    void OnConnectionAdded(object sender, PowerLineConnection conn)
        => JoinComponents(conn.A, conn.B);

    void OnConnectionRemoved(object sender, PowerLineConnection conn)
        => ReorganizeAround(conn.A, conn.B);

    void JoinComponents(PowerLineComponent a, PowerLineComponent b)
    {
        if (!a || a.IsDeleted || !a.IsFinished || !b || b.IsDeleted || !b.IsFinished) { return; }

        var nodeA = a.GetComponent<MechanicalNode>();
        var nodeB = b.GetComponent<MechanicalNode>();
        if (!nodeA || !nodeB) { return; }

        JoinGraphs(nodeA.Graph, nodeB.Graph);
    }

    void JoinWithPowerLinePartners(MechanicalNode node, PowerLineComponent powerLine)
    {
        HashSet<MechanicalGraph> graphs = [];
        if (node.Graph is { } selfGraph)
        {
            graphs.Add(selfGraph);
        }

        foreach (var other in connectionService.GetConnectedPowerLines(powerLine))
        {
            if (!other || other.IsDeleted || !other.IsFinished) { continue; }

            var otherNode = other.GetComponent<MechanicalNode>();
            if (otherNode && otherNode.Graph is { } otherGraph)
            {
                graphs.Add(otherGraph);
            }
        }

        if (graphs.Count > 1)
        {
            graphFactory.Join(graphs);
        }
    }

    void JoinGraphs(MechanicalGraph? a, MechanicalGraph? b)
    {
        if (a is null || b is null || a == b) { return; }
        graphFactory.Join([a, b]);
    }

    void ReorganizeAround(PowerLineComponent a, PowerLineComponent b)
    {
        // Prefer a still-finished node that remains on a graph after the link is gone.
        MechanicalGraph? graph = null;

        if (a && !a.IsDeleted)
        {
            graph = a.GetComponent<MechanicalNode>()?.Graph;
        }

        if (graph is null && b && !b.IsDeleted)
        {
            graph = b.GetComponent<MechanicalNode>()?.Graph;
        }

        if (graph is null) { return; }

        // Split/rebuild using transput edges + power-line edges (via VisitConnectedNodes patch).
        graphReorganizer.Reorganize(graph);
    }
}
