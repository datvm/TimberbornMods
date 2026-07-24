namespace StockpileBalancer.Services;

[BindSingleton]
public class StockpileBalancerService
{
    readonly HashSet<BalancerGroup> groups = [];
    public IReadOnlyCollection<BalancerGroup> Groups => groups;

    readonly Dictionary<Vector3Int, StockpileBalancerComponent> clusterablesByLocations = [];

    public void OnBalancerUpdated(StockpileBalancerComponent balancer)
    {
        // Leave old spatial index + cluster first so neighbor scans never see a stale node.
        RemoveOccupiedBalancers(balancer);
        DetachFromGroup(balancer);

        if (!balancer.IsClusterable)
        {
            return;
        }

        AddOccupiedBalancers(balancer);
        JoinWithNeighbors(balancer);
    }

    void DetachFromGroup(StockpileBalancerComponent balancer)
    {
        var group = balancer.BalancerGroup;
        if (group is null) { return; }

        groups.Remove(group);

        HashSet<StockpileBalancerComponent> remaining = [.. group.Balancers];
        remaining.Remove(balancer);

        foreach (var member in group.Balancers)
        {
            member.BalancerGroup = null;
        }

        // Remaining members may form multiple components without this node.
        ResplitRemaining(remaining);
    }

    void ResplitRemaining(HashSet<StockpileBalancerComponent> remaining)
    {
        while (remaining.Count > 0)
        {
            var start = remaining.First();
            var component = FloodFill(start, remaining);
            if (component.Count <= 1)
            {
                // Singletons do not need a group (nothing to balance).
                continue;
            }

            CreateGroup(component, start.GoodId!);
        }
    }

    void JoinWithNeighbors(StockpileBalancerComponent balancer)
    {
        var neighbors = FindConnectableNeighbors(balancer);
        if (neighbors.Count == 0)
        {
            balancer.BalancerGroup = null;
            return;
        }

        HashSet<StockpileBalancerComponent> members = [balancer];
        HashSet<BalancerGroup> neighborGroups = [];

        foreach (var neighbor in neighbors)
        {
            if (neighbor.BalancerGroup is { } g)
            {
                neighborGroups.Add(g);
            }
            else
            {
                // Neighbor is tracked but alone (no group yet).
                members.Add(neighbor);
            }
        }

        foreach (var g in neighborGroups)
        {
            groups.Remove(g);
            foreach (var member in g.Balancers)
            {
                member.BalancerGroup = null;
                members.Add(member);
            }
        }

        if (members.Count <= 1)
        {
            balancer.BalancerGroup = null;
            return;
        }

        CreateGroup(members, balancer.GoodId!);
    }

    void CreateGroup(IEnumerable<StockpileBalancerComponent> members, string goodId)
    {
        var set = members.ToImmutableHashSet();
        var group = new BalancerGroup(goodId, set);
        foreach (var member in set)
        {
            member.BalancerGroup = group;
        }

        group.MarkDirty();
        groups.Add(group);
    }

    HashSet<StockpileBalancerComponent> FloodFill(
        StockpileBalancerComponent start,
        HashSet<StockpileBalancerComponent> remaining)
    {
        HashSet<StockpileBalancerComponent> components = [];
        Stack<StockpileBalancerComponent> stack = new();

        remaining.Remove(start);
        components.Add(start);
        stack.Push(start);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            foreach (var neighbor in FindConnectableNeighbors(curr))
            {
                if (!remaining.Remove(neighbor)) { continue; }

                components.Add(neighbor);
                stack.Push(neighbor);
            }
        }

        return components;
    }

    List<StockpileBalancerComponent> FindConnectableNeighbors(StockpileBalancerComponent balancer)
    {
        // HorizontalSide never needs ±z (stricter-of-two cannot become AnyTouch).
        var includeVertical = balancer.ConnectionMode != DistroBalancerConnectionMode.HorizontalSide;

        HashSet<StockpileBalancerComponent> candidates = [];
        CollectFaceAdjacent(balancer, includeVertical, candidates);

        List<StockpileBalancerComponent> result = [];
        foreach (var other in candidates)
        {
            if (CanConnect(balancer, other))
            {
                result.Add(other);
            }
        }

        return result;
    }

    /// <summary>
    /// Probe cells just outside the AABB faces (face-adjacent only — no diagonals).
    /// </summary>
    void CollectFaceAdjacent(
        StockpileBalancerComponent balancer,
        bool includeVertical,
        HashSet<StockpileBalancerComponent> candidates)
    {
        var b = balancer.Bounds;

        // ±X faces
        for (var y = b.yMin; y < b.yMax; y++)
        {
            for (var z = b.zMin; z < b.zMax; z++)
            {
                TryAddCandidate(new(b.xMax, y, z), balancer, candidates);
                TryAddCandidate(new(b.xMin - 1, y, z), balancer, candidates);
            }
        }

        // ±Y faces
        for (var x = b.xMin; x < b.xMax; x++)
        {
            for (var z = b.zMin; z < b.zMax; z++)
            {
                TryAddCandidate(new(x, b.yMax, z), balancer, candidates);
                TryAddCandidate(new(x, b.yMin - 1, z), balancer, candidates);
            }
        }

        if (!includeVertical) { return; }

        // ±Z faces
        for (var x = b.xMin; x < b.xMax; x++)
        {
            for (var y = b.yMin; y < b.yMax; y++)
            {
                TryAddCandidate(new(x, y, b.zMax), balancer, candidates);
                TryAddCandidate(new(x, y, b.zMin - 1), balancer, candidates);
            }
        }
    }

    void TryAddCandidate(
        Vector3Int cell,
        StockpileBalancerComponent self,
        HashSet<StockpileBalancerComponent> candidates)
    {
        if (clusterablesByLocations.TryGetValue(cell, out var other) && other != self)
        {
            candidates.Add(other);
        }
    }

    static bool CanConnect(StockpileBalancerComponent a, StockpileBalancerComponent b)
    {
        if (a.GoodId is null || a.GoodId != b.GoodId) { return false; }

        // Stricter of the two buildings' modes.
        var mode = (DistroBalancerConnectionMode)Math.Min(
            (int)a.ConnectionMode,
            (int)b.ConnectionMode);

        return mode switch
        {
            DistroBalancerConnectionMode.HorizontalSide => HasCompleteSideContact(a.Bounds, b.Bounds),
            // Face probe already proved ≥1 shared face (incl. vertical); no diagonals.
            DistroBalancerConnectionMode.AnyTouch => true,
            _ => false,
        };
    }

    /// <summary>
    /// Either AABB has one full horizontal side flush against the other
    /// (smaller facade counts — e.g. 1×1 fully on a larger wall).
    /// </summary>
    static bool HasCompleteSideContact(BoundsInt a, BoundsInt b)
        => SideFullyAgainst(a, b) || SideFullyAgainst(b, a);

    /// <summary>
    /// Self has a ±X or ±Y face entirely covered by other.
    /// BoundsInt is half-open: [min, max). Adjacent means self.max == other.min on that axis.
    /// </summary>
    static bool SideFullyAgainst(BoundsInt self, BoundsInt other)
    {
        var coversYZ = other.yMin <= self.yMin && other.yMax >= self.yMax
            && other.zMin <= self.zMin && other.zMax >= self.zMax;
        if (coversYZ && (other.xMin == self.xMax || self.xMin == other.xMax))
        {
            return true;
        }

        var coversXZ = other.xMin <= self.xMin && other.xMax >= self.xMax
            && other.zMin <= self.zMin && other.zMax >= self.zMax;
        return coversXZ && (other.yMin == self.yMax || self.yMin == other.yMax);
    }

    void AddOccupiedBalancers(StockpileBalancerComponent balancer)
    {
        foreach (var cell in balancer.Bounds.allPositionsWithin)
        {
            clusterablesByLocations[cell] = balancer;
        }
    }

    void RemoveOccupiedBalancers(StockpileBalancerComponent balancer)
    {
        foreach (var cell in balancer.Bounds.allPositionsWithin)
        {
            clusterablesByLocations.Remove(cell);
        }
    }

}
