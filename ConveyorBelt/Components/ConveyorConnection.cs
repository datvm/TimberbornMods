namespace ConveyorBelt.Components;

public class ConveyorConnection(IBlockService blockService) : BaseComponent, IInitializableEntity
{
    ConveyorConnection?[] neighbors = [];
    readonly HashSet<Inventory> ignoredInventories = [];

    public ImmutableArray<BeltPort> Ports { get; private set; } = [];

    public IEnumerable<ConveyorConnection> Connected
    {
        get
        {
            foreach (var n in neighbors)
            {
                if (n is { } connected)
                {
                    yield return connected;
                }
            }
        }
    }

    public void InitializeEntity()
    {
        CollectIgnoredInventories();

        if (!HasComponent<BuildingSpec>()) { return; }

        var bo = GetComponent<BlockObject>();
        if (!bo) { return; }

        Ports = [.. Expand(GetLocalPorts(bo), bo)];
        neighbors = new ConveyorConnection[Ports.Length];

        if (bo.IsFinished)
        {
            RefreshNeighbors();
            foreach (var n in Connected)
            {
                n.RefreshNeighbors();
            }
        }
    }

    public ConveyorConnection? NeighborAt(Vector3Int target)
    {
        for (var i = 0; i < Ports.Length; i++)
        {
            if (Ports[i].Target == target)
            {
                return neighbors[i];
            }
        }

        return null;
    }

    public bool HasFacingPort(Vector3Int from, BeltPortKind kind = BeltPortKind.Both)
    {
        foreach (var port in Ports)
        {
            if (!port.Faces(from)) { continue; }
            if (kind == BeltPortKind.Both ? port.Kind != BeltPortKind.None : port.Allows(kind))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<Inventory> GetUsableInventories()
    {
        var inventories = GetComponent<Inventories>();
        if (!inventories) { yield break; }

        foreach (var inv in inventories.EnabledInventories)
        {
            if (ignoredInventories.Contains(inv)) { continue; }
            yield return inv;
        }
    }

    void CollectIgnoredInventories()
    {
        ignoredInventories.Clear();
        List<IIgnoredBeltInventory> providers = [];
        GetComponents(providers);
        foreach (var provider in providers)
        {
            foreach (var inv in provider.GetIgnoredInventories())
            {
                if (inv)
                {
                    ignoredInventories.Add(inv);
                }
            }
        }
    }

    public void RefreshNeighbors()
    {
        for (var i = 0; i < Ports.Length; i++)
        {
            neighbors[i] = FindNeighbor(Ports[i]);
        }
    }

    public void ClearNeighbors()
    {
        Array.Clear(neighbors, 0, neighbors.Length);
    }

    public static ImmutableArray<BeltPortSpec> GenerateFromOccupancy(IEnumerable<Vector3Int> occupied)
    {
        var cells = occupied.ToHashSet();
        List<BeltPortSpec> ports = [];

        foreach (var cell in cells)
        {
            var directions = Directions3D.None;
            foreach (var n in Deltas.Neighbors6Vector3Int)
            {
                if (!cells.Contains(cell + n))
                {
                    directions |= Direction3DExtensions.FromOffset(n).ToDirections3D();
                }
            }

            if (directions == Directions3D.None) { continue; }

            ports.Add(new()
            {
                Coordinates = cell,
                Directions = directions,
                Kind = BeltPortKind.Both,
            });
        }

        return [.. ports];
    }

    IEnumerable<BeltPortSpec> GetLocalPorts(BlockObject bo)
    {
        var connectionSpec = GetComponent<ConveyorConnectionSpec>();
        if (connectionSpec is not null)
        {
            return connectionSpec.Ports;
        }

        var beltSpec = GetComponent<ConveyorBeltSpec>();
        if (beltSpec is not null)
        {
            return
            [
                Toward(beltSpec.InputCoordinates, BeltPortKind.In),
                Toward(beltSpec.OutputCoordinates, BeltPortKind.Out),
            ];
        }

        var junctionSpec = GetComponent<ConveyorBeltJunctionSpec>();
        if (junctionSpec is not null)
        {
            return
            [
                .. junctionSpec.InputCoordinates.Select(c => Toward(c, BeltPortKind.In)),
                .. junctionSpec.OutputCoordinates.Select(c => Toward(c, BeltPortKind.Out)),
            ];
        }

        return GenerateFromOccupancy(bo.Blocks.GetOccupiedCoordinates());
    }

    ConveyorConnection? FindNeighbor(BeltPort port)
    {
        var target = port.Target;
        if (!blockService.Contains(target)) { return null; }

        var other = blockService.GetFirstObjectWithComponentAt<ConveyorConnection>(target);
        if (!other || other == this) { return null; }

        var otherBo = other.GetComponent<BlockObject>();
        if (!otherBo || !otherBo.IsFinished) { return null; }

        var needed = port.Kind switch
        {
            BeltPortKind.In => BeltPortKind.Out,
            BeltPortKind.Out => BeltPortKind.In,
            BeltPortKind.Both => BeltPortKind.Both,
            _ => BeltPortKind.None,
        };
        if (needed == BeltPortKind.None || !other.HasFacingPort(port.Coordinates, needed)) { return null; }

        return other;
    }

    static BeltPortSpec Toward(Vector3Int neighborOffset, BeltPortKind kind) => new()
    {
        Coordinates = Vector3Int.zero,
        Directions = Direction3DExtensions.FromOffset(neighborOffset).ToDirections3D(),
        Kind = kind,
    };

    static IEnumerable<BeltPort> Expand(IEnumerable<BeltPortSpec> localPorts, BlockObject bo)
    {
        foreach (var spec in localPorts)
        {
            foreach (var dir in spec.Directions)
            {
                yield return new(
                    bo.TransformCoordinates(spec.Coordinates),
                    bo.TransformDirection(dir),
                    spec.Kind);
            }
        }
    }
}
