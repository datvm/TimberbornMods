namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltService(
    RecoveredGoodStackSpawner goodStackSpawner,
    IDayNightCycle dayNightCycle,
    ILoc t,
    EventBus eb,
    IGoodService goods,
    IEnumerable<IConveyorBeltSpeedModifier> speedModifierSource
) : ILoadableSingleton
{
    public const string LiquidGoodType = "Liquid";

    public readonly ILoc t = t;
    readonly IConveyorBeltSpeedModifier[] speedModifiers = [.. speedModifierSource];
    readonly Dictionary<string, string> goodTypes = [];
    readonly Dictionary<Vector3Int, ConveyorBeltComponent> belts = [];
    readonly Dictionary<Vector3Int, ConveyorBeltJunction> junctions = [];

    public float HoursPerTick { get; private set; }
    public float SpeedMultiplier
    {
        get
        {
            var m = 1f;
            foreach (var mod in speedModifiers)
            {
                m *= mod.SpeedMultiplier;
            }
            return m;
        }
    }

    public IReadOnlyDictionary<Vector3Int, ConveyorBeltComponent> Belts => belts;
    public IReadOnlyDictionary<Vector3Int, ConveyorBeltJunction> Junctions => junctions;

    public void Load()
    {
        HoursPerTick = dayNightCycle.TicksToHours(1);
        eb.Register(this);
        foreach (var id in goods.Goods)
        {
            goodTypes[id] = goods.GetGood(id).GoodType;
        }
    }

    [OnEvent]
    public void OnFinished(EnteredFinishedStateEvent e) => AddObject(e.BlockObject);

    [OnEvent]
    public void OnDestroyed(ExitedFinishedStateEvent e) => RemoveObject(e.BlockObject);

    void AddObject(BlockObject? bo)
    {
        if (!bo)
        {
            return;
        }

        var block = bo!;
        if (block.GetComponentOrNull<ConveyorBeltJunction>() is { } jc)
        {
            junctions[jc.Coordinates] = jc;
        }

        Link(block);
    }

    void RemoveObject(BlockObject? bo)
    {
        if (!bo)
        {
            return;
        }

        var block = bo!;
        if (block.GetComponentOrNull<ConveyorBeltJunction>() is { } jc)
        {
            junctions.Remove(jc.Coordinates);
        }

        Unlink(block);
    }

    static void Link(BlockObject bo)
    {
        if (bo.GetComponentOrNull<ConveyorConnection>() is not { } conn)
        {
            return;
        }

        conn.CacheModules();
        conn.RefreshNeighbors();
        foreach (var n in conn.Connected)
        {
            n.RefreshNeighbors();
        }
    }

    static void Unlink(BlockObject bo)
    {
        if (bo.GetComponentOrNull<ConveyorConnection>() is not { } conn)
        {
            return;
        }

        var others = conn.Connected.ToArray();
        conn.ClearNeighbors();
        foreach (var n in others)
        {
            n.RefreshNeighbors();
        }
    }

    public void RegisterBelt(ConveyorBeltComponent belt)
    {
        var coords = belt.Coordinates;
        if (belts.ContainsKey(coords))
        {
            throw new InvalidOperationException($"A belt is already registered at coordinates {coords}");
        }

        belt.Connection.CacheModules();
        belts[coords] = belt;
    }

    public void UnregisterBelt(ConveyorBeltComponent belt)
    {
        var coords = belt.Coordinates;
        if (!belts.Remove(coords))
        {
            throw new InvalidOperationException($"No belt is registered at coordinates {coords}");
        }
    }

    public void SpawnGoods(Vector3Int pos, IEnumerable<GoodAmount> goods)
        => goodStackSpawner.AddAwaitingGoods(pos, goods);

    public bool TryTransferContentOut(ConveyorBeltComponent belt, string goodId)
    {
        var dst = belt.Connection.NeighborAt(belt.OutputCoordinates);
        if (dst is null)
        {
            return false;
        }

        if (dst.Belt is { } nextBelt && TryMovingIntoBelt(belt, nextBelt, belt.Coordinates, goodId))
        {
            return true;
        }

        return TryMovingIntoInventory(belt, dst, goodId);
    }

    public bool TryGrabContentIntoBelt(ConveyorBeltComponent belt)
    {
        var src = belt.Connection.NeighborAt(belt.InputCoordinates);
        if (src is null)
        {
            return false;
        }

        return TryGrabFromInventory(belt, src);
    }

    public string GetGoodType(string goodId)
    {
        if (goodTypes.TryGetValue(goodId, out var type))
        {
            return type;
        }

        type = goods.GetGood(goodId).GoodType;
        goodTypes[goodId] = type;
        return type;
    }

    public IEnumerable<GoodSpec> GetQualifiedGoods(ConveyorBeltComponent belt)
    {
        foreach (var id in goods.Goods)
        {
            var g = goods.GetGood(id);
            if (belt.ForbidsGoodType(g.GoodType))
            {
                continue;
            }

            yield return g;
        }
    }

    static bool TryMovingIntoBelt(ConveyorBeltComponent src, ConveyorBeltComponent dst, Vector3Int srcCoords, string goodId)
    {
        if (!dst.CanAcceptItem(goodId))
        {
            return false;
        }

        if (!dst.IsInputCoordinates(srcCoords))
        {
            return false;
        }

        var item = src.Pop();
        dst.Push(item.GoodId);
        return true;
    }

    static bool TryMovingIntoInventory(ConveyorBeltComponent src, ConveyorConnection conn, string goodId)
    {
        foreach (var inv in conn.GetUsableInventories())
        {
            if (!inv.IsInput)
            {
                continue;
            }

            if (!inv.HasUnreservedCapacity(goodId))
            {
                continue;
            }

            var item = src.Pop();
            inv.GiveExisting(new(item.GoodId, 1));
            return true;
        }

        return false;
    }

    static bool TryGrabFromInventory(ConveyorBeltComponent belt, ConveyorConnection conn)
    {
        foreach (var inv in conn.GetUsableInventories())
        {
            if (!inv.IsOutput)
            {
                continue;
            }

            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount == 0)
                {
                    continue;
                }

                if (!belt.IsValidGood(stock.GoodId))
                {
                    continue;
                }

                belt.Push(stock.GoodId);
                inv.TakeExisting(new(stock.GoodId, 1));
                return true;
            }
        }

        return false;
    }
}
