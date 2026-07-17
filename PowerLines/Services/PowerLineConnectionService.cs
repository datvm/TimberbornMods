namespace PowerLines.Services;

[BindSingleton]
public class PowerLineConnectionService(
    ISingletonLoader loader,
    EntityRegistry entities
) : ILoadableSingleton, ISaveableSingleton, IPostLoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(PowerLineConnectionService));
    static readonly PropertyKey<string> ConnectionsKey = new("Connections");

    readonly Dictionary<PowerLineComponent, List<PowerLineConnection>> connections = [];
    string? loadedString;

    public event EventHandler<PowerLineConnection>? ConnectionAdded;
    public event EventHandler<PowerLineConnection>? ConnectionRemoved;

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }
        loadedString = s.Get(ConnectionsKey);
    }

    public void Connect(PowerLineComponent a, PowerLineComponent b)
    {
        var err = TryConnecting(a, b);
        if (err is not null)
        {
            throw new InvalidOperationException($"Failed to connect {a} and {b}: {err}");
        }
    }

    public void DisconnectConnection(PowerLineConnection conn)
    {
        if (connections.TryGetValue(conn.A, out var aConns))
        {
            aConns.Remove(conn);
        }
        if (connections.TryGetValue(conn.B, out var bConns))
        {
            bConns.Remove(conn);
        }

        ConnectionRemoved?.Invoke(this, conn);
    }

    public void DisconnectAllComponentConnections(PowerLineComponent comp)
    {
        if (!connections.TryGetValue(comp, out var conns)) { return; }

        foreach (var conn in conns.ToArray())
        {
            DisconnectConnection(conn);
        }
        connections.Remove(comp);
    }

    public IReadOnlyList<PowerLineConnection> GetConnections(PowerLineComponent powerline) 
        => connections.TryGetValue(powerline, out var conns) ? conns : [];

    public IEnumerable<PowerLineComponent> GetConnectedPowerLines(PowerLineComponent powerline)
        => GetConnections(powerline).Select(c => c.A == powerline ? c.B : c.A);

    public bool HasFreeSlots(PowerLineComponent powerline)
        => GetConnections(powerline).Count < powerline.MaxConnections;

    public PowerLineConnectionCheck EvaluateConnection(PowerLineComponent a, PowerLineComponent b)
    {
        // Distance is the shortest pairing between any of either side's connection points
        var distance = a.ShortestDistanceTo(b);
        // Either max length will increase the distance limit
        var maxDistance = Mathf.Max(a.MaxConnectionLength, b.MaxConnectionLength);
        var distanceOk = distance <= maxDistance;
        var hasSlot = HasFreeSlots(a) && HasFreeSlots(b);
        var alreadyConnected = GetConnections(a).Any(c => c.A == b || c.B == b);

        return new(
            CanConnect: distanceOk && hasSlot && !alreadyConnected && a != b && a && b && !a.IsDeleted && !b.IsDeleted,
            Distance: distance,
            MaxDistance: maxDistance,
            DistanceOk: distanceOk,
            HasSlot: hasSlot);
    }

    public string? CanConnect(PowerLineComponent a, PowerLineComponent b)
    {
        if (!a || a.IsDeleted)
        {
            return $"Power Line {a} does not exist";
        }

        if (!b || b.IsDeleted)
        {
            return $"Power Line {b} does not exist";
        }

        if (a == b)
        {
            return $"Cannot connect a power line to itself ({a})";
        }

        var check = EvaluateConnection(a, b);
        if (check.CanConnect) { return null; }

        if (!check.DistanceOk)
        {
            return $"Distance {check.Distance:0.##} exceeds max connection length of {check.MaxDistance:0.##}";
        }

        if (!check.HasSlot)
        {
            return "No free connection slots";
        }

        return $"Power Line {a} is already connected to {b}";
    }

    string? TryConnecting(PowerLineComponent a, PowerLineComponent b)
    {
        var err = CanConnect(a, b);
        if (err is not null) { return err; }

        var conn = new PowerLineConnection(a, b).Normalize();

        connections.GetOrAdd(a, () => []).Add(conn);
        connections.GetOrAdd(b, () => []).Add(conn);

        ConnectionAdded?.Invoke(this, conn);

        return null;
    }

    public void PostLoad()
    {
        var idPairs = ParseIds(loadedString);
        if (idPairs.Length == 0) { return; }

        var grps = idPairs.GroupBy(p => p.Item1);
        foreach (var grp in grps)
        {
            var a = GetPowerLine(grp.Key);
            if (a is null || a.IsDeleted)
            {
                Debug.LogWarning($"Power Line {grp.Key} no longer exists, skipping connections");
                continue;
            }

            foreach (var other in grp)
            {
                var b = GetPowerLine(other.Item2);
                if (b is null || b.IsDeleted)
                {
                    Debug.LogWarning($"Power Line {other.Item2} no longer exists, skipping connection");
                    continue;
                }

                var err = TryConnecting(a, b);
                if (err is not null)
                {
                    Debug.LogWarning($"Failed to restore connection between {a} and {b}: {err}");
                }
            }
        }
    }

    PowerLineComponent? GetPowerLine(Guid id)
    {
        var e = entities.GetEntity(id);
        if (!e) { return null; }

        var plc = e.GetComponent<PowerLineComponent>();
        return plc ? plc : null;
    }

    (Guid, Guid)[] ParseIds(string? loaded)
    {
        if (string.IsNullOrEmpty(loaded)) { return []; }

        var ids = loaded.Split('|');
        var pairs = new (Guid, Guid)[ids.Length / 2];
        for (var i = 0; i < ids.Length; i += 2)
        {
            var a = Guid.Parse(ids[i]);
            var b = Guid.Parse(ids[i + 1]);

            pairs[i / 2] = (a, b);
        }

        return pairs;
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        var conn = connections.Values
            .SelectMany(x => x)
            .ToHashSet();
        s.Set(ConnectionsKey, string.Join("|", conn.Select(c => $"{c.GuidA}|{c.GuidB}")));
    }

}
