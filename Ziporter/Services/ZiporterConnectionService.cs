namespace Ziporter.Services;

public readonly record struct EdgePair(NavMeshEdge From, NavMeshEdge To);

public class ZiporterConnectionService(
    ZiporterNavGroupService navGroup,
    INavMeshService navMeshService
)
{
    public const float TravelCost = float.Epsilon;

    readonly HashSet<ZiporterConnection> registry = [];
    readonly Dictionary<ZiporterConnection, ZiporterConnection> connections = [];
    readonly Dictionary<ZiporterConnection, EdgePair> activePaths = [];

    public bool HasActivePath(ZiporterConnection left, ZiporterConnection right)
        => activePaths.ContainsKey(left) || activePaths.ContainsKey(right);

    public void Connect(ZiporterConnection left, ZiporterConnection right)
    {
        ValidateNewConnection(left, right);

        connections.Add(left, right);
        connections.Add(right, left);

        // New connection, i.e. will not happen when registering/loading
        if (!left.IsConnected)
        {
            left.Connect(right);
        }

        if (!right.IsConnected)
        {
            right.Connect(left);
        }
    }

    public void Disconnect(ZiporterConnection connection)
    {
        // Should not happen but if there is consistency between the two, remove everything
        var other = connection.ConnectedZiporter;
        connections.TryGetValue(connection, out var recordedOther);

        connection.Disconnect();
        connections.Remove(connection);
        RemovePath(connection);

        if (other)
        {
            other.Disconnect();
            connections.Remove(other);
            RemovePath(other);
        }

        if (other != recordedOther && recordedOther)
        {
            recordedOther.Disconnect();
            connections.Remove(recordedOther);
            RemovePath(recordedOther);
        }
    }

    public void TogglePath(ZiporterConnection left, ZiporterConnection right, bool activate)
    {
        var hasPath = HasActivePath(left, right);
        if (hasPath == activate) { return; }

        if (activate)
        {
            AddPath(left, right);
        }
        else
        {
            RemovePath(left, right);
        }
    }

    public bool CanBeConnected(ZiporterConnection? left, ZiporterConnection? right) =>
        left && right && left != right 
        && !left.IsConnected && !right.IsConnected
        && left.IsFinished && right.IsFinished;

    void AddPath(ZiporterConnection left, ZiporterConnection right)
    {
        var edge = CreateEdges(left, right);
        navMeshService.AddEdge(edge.From);
        navMeshService.AddEdge(edge.To);

        activePaths.Add(left, edge);
    }

    void RemovePath(ZiporterConnection left, ZiporterConnection right)
    {
        RemovePath(left);
        RemovePath(right);
    }

    void RemovePath(ZiporterConnection conn)
    {
        if (!activePaths.TryGetValue(conn, out var edge)) { return; }

        navMeshService.RemoveEdge(edge.From);
        navMeshService.RemoveEdge(edge.To);
        activePaths.Remove(conn);
    }

    public void Register(ZiporterConnection connection)
    {
        registry.Add(connection);
        connection.OnActiveChanged += Connection_OnActiveChanged;

        var other = connection.ConnectedZiporter;
        if (!other || !registry.Contains(other)) { return; }

        // Connect the two
        Connect(connection, other);
    }

    public void Unregister(ZiporterConnection connection)
    {
        registry.Remove(connection);
        connection.OnActiveChanged -= Connection_OnActiveChanged;

        if (connections.TryGetValue(connection, out var other))
        {
            connection.Disconnect();
            other.Disconnect();

            connections.Remove(connection);
            connections.Remove(other);
        }
    }

    void ValidateNewConnection(ZiporterConnection left, ZiporterConnection right)
    {
        if (connections.TryGetValue(left, out var connectingLeft))
        {
            throw new InvalidOperationException(
                $"Cannot connect {left} to {right} because it is already connected to {connectingLeft}.");
        }

        if (connections.TryGetValue(right, out var connectingRight))
        {
            throw new InvalidOperationException(
                $"Cannot connect {right} to {left} because it is already connected to {connectingRight}.");
        }

        if (left.ConnectedZiporter && left.ConnectedZiporter != right)
        {
            throw new InvalidOperationException(
                $"Cannot connect {left} to {right} because it is already connected to {left.ConnectedZiporter}.");
        }

        if (right.ConnectedZiporter && right.ConnectedZiporter != left)
        {
            throw new InvalidOperationException(
                $"Cannot connect {right} to {left} because it is already connected to {right.ConnectedZiporter}.");
        }
    }

    EdgePair CreateEdges(ZiporterConnection left, ZiporterConnection right)
    {
        var start = left.AnchorPoint.FloorToInt();
        var end = right.AnchorPoint.FloorToInt();
        var regularGroupId = navGroup.GroupId;

        NavMeshEdge edgeFrom = NavMeshEdge.CreateGrouped(start, end, regularGroupId, true, TravelCost);
        NavMeshEdge edgeTo = NavMeshEdge.CreateGrouped(end, start, regularGroupId, true, TravelCost);
        return new EdgePair(edgeFrom, edgeTo);
    }

    void Connection_OnActiveChanged(object sender, EventArgs e)
    {
        var conn = (ZiporterConnection)sender;
        var other = conn.ConnectedZiporter;
        if (other)
        {
            TogglePath(conn, other, conn.IsActive);
        }
    }

}
