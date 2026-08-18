namespace TimberPipes.Services;

[BindSingleton]
public class PipeRegistry
{

    readonly Dictionary<Vector3Int, BuildingPipe> pipes = [];
    readonly Dictionary<PipePortDefinition, BuildingPipe> portOwners = [];

    readonly HashSet<PipeGraph> graphs = [];
    public IReadOnlyCollection<PipeGraph> Graphs => graphs;

    public bool TryGetGraph(Vector3Int coordinates, [NotNullWhen(true)] out PipeGraph? graph)
    {
        if (TryGetPipe(coordinates, out var pipe))
        {
            graph = pipe.Graph;
            return graph is not null;
        }
        graph = null;
        return false;
    }

    public bool TryGetPipe(Vector3Int coordinates, [NotNullWhen(true)] out BuildingPipe? pipe)
        => pipes.TryGetValue(coordinates, out pipe);

    internal void Register(BuildingPipe buildingPipe)
    {
        if (pipes.ContainsKey(buildingPipe.Coordinates))
        {
            throw new InvalidOperationException($"Another pipe already registered at {buildingPipe.Coordinates}");
        }

        if (buildingPipe.Ports is null)
        {
            throw new InvalidOperationException($"Cannot register pipe at {buildingPipe.Coordinates} before ports are initialized");
        }

        pipes[buildingPipe.Coordinates] = buildingPipe;
        IndexPorts(buildingPipe);
        RebuildGraph(buildingPipe, adding: true);
    }

    internal void Unregister(BuildingPipe buildingPipe)
    {
        if (!pipes.Remove(buildingPipe.Coordinates))
        {
            throw new InvalidOperationException($"No pipe registered at {buildingPipe.Coordinates}");
        }

        RebuildGraph(buildingPipe, adding: false);
        UnindexPorts(buildingPipe);
    }

    void RebuildGraph(BuildingPipe pipe, bool adding)
    {
        if (adding)
        {
            ConnectPorts(pipe);
            if (pipe.IsTransportPipe)
            {
                AssignMergedGraph(pipe);
            }
            return;
        }

        var oldGraph = pipe.Graph;
        DisconnectPorts(pipe);
        pipe.Graph = null;

        if (!pipe.IsTransportPipe || oldGraph is null)
        {
            return;
        }

        graphs.Remove(oldGraph);

        List<BuildingPipe> remaining = [];
        foreach (var other in oldGraph.Pipes.Values)
        {
            if (other == pipe)
            {
                continue;
            }

            other.Graph = null;
            remaining.Add(other);
        }

        AssignComponents(remaining, oldGraph.Contaminated);
    }

    void ConnectPorts(BuildingPipe pipe)
    {
        if (pipe.Ports is not { } ports)
        {
            return;
        }

        foreach (var port in ports.Values)
        {
            if (port.IsConnected)
            {
                continue;
            }

            var opposite = port.GetOppositePortDefinition();
            if (!TryGetOwnedPort(opposite, out var neighborPipe, out var neighborPort))
            {
                continue;
            }

            if (neighborPipe == pipe)
            {
                continue;
            }

            if (neighborPort.IsConnected)
            {
                throw new InvalidOperationException(
                    $"Port {opposite} on {neighborPipe.Coordinates} is already connected while matching {port.Definition} on {pipe.Coordinates}");
            }

            var connection = new PipePortConnection(port, neighborPort);
            port.Connection = connection;
            neighborPort.Connection = connection;
        }
    }

    void DisconnectPorts(BuildingPipe pipe)
    {
        if (pipe.Ports is not { } ports)
        {
            return;
        }

        foreach (var port in ports.Values)
        {
            if (port.Connection is not { } connection)
            {
                continue;
            }

            var other = connection.GetOther(port);
            other.Connection = null;
            port.Connection = null;
        }
    }

    void AssignMergedGraph(BuildingPipe pipe)
    {
        var component = FloodFillTransport(pipe);

        var contaminated = false;
        HashSet<PipeGraph> oldGraphs = [];
        foreach (var member in component)
        {
            if (member.Graph is not { } graph)
            {
                continue;
            }

            contaminated |= graph.Contaminated;
            oldGraphs.Add(graph);
        }

        foreach (var graph in oldGraphs)
        {
            graphs.Remove(graph);
        }

        CreateGraph(component, contaminated);
    }

    void AssignComponents(List<BuildingPipe> pipesToAssign, bool contaminated)
    {
        HashSet<BuildingPipe> remaining = [.. pipesToAssign];
        while (remaining.Count > 0)
        {
            BuildingPipe? start = null;
            foreach (var candidate in remaining)
            {
                start = candidate;
                break;
            }

            var component = FloodFillTransport(start!);
            foreach (var member in component)
            {
                remaining.Remove(member);
            }

            CreateGraph(component, contaminated);
        }
    }

    void CreateGraph(List<BuildingPipe> component, bool contaminated)
    {
        Dictionary<Vector3Int, BuildingPipe> map = [];
        foreach (var member in component)
        {
            map[member.Coordinates] = member;
        }

        var graph = new PipeGraph(map.ToFrozenDictionary())
        {
            Contaminated = contaminated,
        };
        graphs.Add(graph);

        foreach (var member in component)
        {
            member.Graph = graph;
        }
    }

    List<BuildingPipe> FloodFillTransport(BuildingPipe start)
    {
        List<BuildingPipe> result = [];
        if (!start.IsTransportPipe)
        {
            return result;
        }

        Queue<BuildingPipe> queue = [];
        HashSet<BuildingPipe> visited = [];
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            if (current.Ports is not { } ports)
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (port.ConnectedPort is not { } otherPort)
                {
                    continue;
                }

                if (!portOwners.TryGetValue(otherPort.Definition, out var otherPipe))
                {
                    continue;
                }

                // Internal buildings exchange fluid via ports but do not merge pipe graphs.
                if (!otherPipe.IsTransportPipe)
                {
                    continue;
                }

                if (visited.Add(otherPipe))
                {
                    queue.Enqueue(otherPipe);
                }
            }
        }

        return result;
    }

    bool TryGetOwnedPort(
        PipePortDefinition definition,
        [NotNullWhen(true)] out BuildingPipe? owner,
        [NotNullWhen(true)] out PipePort? port)
    {
        if (portOwners.TryGetValue(definition, out owner)
            && owner.Ports is { } ports
            && ports.TryGetValue(definition, out port))
        {
            return true;
        }

        owner = null;
        port = null;
        return false;
    }

    void IndexPorts(BuildingPipe buildingPipe)
    {
        if (buildingPipe.Ports is not { } ports)
        {
            return;
        }

        foreach (var def in ports.Keys)
        {
            if (!portOwners.TryAdd(def, buildingPipe))
            {
                throw new InvalidOperationException(
                    $"Port {def} is already owned by another building while registering {buildingPipe.Coordinates}");
            }
        }
    }

    void UnindexPorts(BuildingPipe buildingPipe)
    {
        if (buildingPipe.Ports is not { } ports)
        {
            return;
        }

        foreach (var def in ports.Keys)
        {
            portOwners.Remove(def);
        }
    }

}
