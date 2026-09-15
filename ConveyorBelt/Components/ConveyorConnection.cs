namespace ConveyorBelt.Components;

public class ConveyorConnection(IBlockService blockService) : BaseComponent, IAwakableComponent, IInitializableEntity
{
    ConveyorConnection?[] neighbors = [];
    Dictionary<Vector3Int, int> portIndexByTarget = [];
    readonly HashSet<Inventory> ignoredInventories = [];

#nullable disable
    BlockObject blockObject;
    Inventories inventories;
#nullable enable

    ConveyorBeltComponent? belt;
    ConveyorBeltJunction? junction;

    public BlockObject BlockObject => blockObject;
    public Inventories Inventories => inventories;
    public ConveyorBeltComponent? Belt => belt;
    public ConveyorBeltJunction? Junction => junction;

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

    public void Awake() => CacheModules();

    internal void CacheModules()
    {
        blockObject = GetComponent<BlockObject>();
        inventories = this.GetComponentOrNull<Inventories>();
        belt = this.GetComponentOrNull<ConveyorBeltComponent>();
        junction = this.GetComponentOrNull<ConveyorBeltJunction>();
    }

    public void InitializeEntity()
    {
        CacheModules();
        CollectIgnoredInventories();

        if (!HasComponent<BuildingSpec>() || !blockObject)
        {
            return;
        }

        Ports = [.. Expand(GetLocalPorts(blockObject), blockObject)];
        neighbors = new ConveyorConnection[Ports.Length];
        portIndexByTarget = new Dictionary<Vector3Int, int>(Ports.Length);
        for (var i = 0; i < Ports.Length; i++)
        {
            portIndexByTarget[Ports[i].Target] = i;
        }

        if (blockObject.IsFinished)
        {
            RefreshNeighbors();
            foreach (var n in Connected)
            {
                n.RefreshNeighbors();
            }
        }
    }

    public ConveyorConnection? NeighborAt(Vector3Int target)
        => portIndexByTarget.TryGetValue(target, out var i) ? neighbors[i] : null;

    public bool HasFacingPort(Vector3Int from, BeltPortKind kind = BeltPortKind.Both)
        => BeltPortIo.HasFacingPort(Ports, from, kind);

    public IEnumerable<Inventory> GetUsableInventories()
    {
        if (!inventories)
        {
            yield break;
        }

        foreach (var inv in inventories.EnabledInventories)
        {
            if (ignoredInventories.Contains(inv))
            {
                continue;
            }

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

            if (directions == Directions3D.None)
            {
                continue;
            }

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
        if (!blockService.Contains(target))
        {
            return null;
        }

        var other = blockService.GetFirstObjectWithComponentAt<ConveyorConnection>(target);
        if (!other || other == this)
        {
            return null;
        }

        other.CacheModules();
        if (!other.blockObject || !other.blockObject.IsFinished)
        {
            return null;
        }

        var needed = BeltPortIo.RequiredNeighborKind(port.Kind);
        if (needed == BeltPortKind.None || !other.HasFacingPort(port.Coordinates, needed))
        {
            return null;
        }

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
