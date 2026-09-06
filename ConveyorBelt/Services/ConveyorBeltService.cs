namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltService(
    RecoveredGoodStackSpawner goodStackSpawner,
    IDayNightCycle dayNightCycle,
    ILoc t,
    EventBus eb,
    IGoodService goods,
    IEnumerable<IConveyorBeltSpeedModifier> speedModifiers
) : ILoadableSingleton
{
    public readonly ILoc t = t;

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
    readonly Dictionary<Vector3Int, ConveyorBeltComponent> belts = [];
    readonly Dictionary<Vector3Int, ConveyorBeltJunction> junctions = [];

    public IReadOnlyDictionary<Vector3Int, ConveyorBeltComponent> Belts => belts;
    public IReadOnlyDictionary<Vector3Int, ConveyorBeltJunction> Junctions => junctions;

    public void Load()
    {
        HoursPerTick = dayNightCycle.TicksToHours(1);
        eb.Register(this);
    }

    [OnEvent]
    public void OnFinished(EnteredFinishedStateEvent e) => AddObject(e.BlockObject);

    [OnEvent]
    public void OnDestroyed(ExitedFinishedStateEvent e) => RemoveObject(e.BlockObject);

    void AddObject(BlockObject? bo)
    {
        if (!bo) { return; }

        var jc = bo!.GetComponent<ConveyorBeltJunction>();
        if (jc)
        {
            junctions[jc.Coordinates] = jc;
        }

        Link(bo);
    }

    void RemoveObject(BlockObject? bo)
    {
        if (!bo) { return; }

        var jc = bo!.GetComponent<ConveyorBeltJunction>();
        if (jc)
        {
            junctions.Remove(jc.Coordinates);
        }

        Unlink(bo);
    }

    static void Link(BlockObject bo)
    {
        var conn = bo.GetComponent<ConveyorConnection>();
        if (!conn) { return; }

        conn.RefreshNeighbors();
        foreach (var n in conn.Connected)
        {
            n.RefreshNeighbors();
        }
    }

    static void Unlink(BlockObject bo)
    {
        var conn = bo.GetComponent<ConveyorConnection>();
        if (!conn) { return; }

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
        var conn = belt.GetComponent<ConveyorConnection>();
        var dst = conn ? conn.NeighborAt(belt.OutputCoordinates) : null;
        if (!dst) { return false; }

        var nextBelt = dst!.GetComponent<ConveyorBeltComponent>();
        if (nextBelt && TryMovingIntoBelt(belt, nextBelt, belt.Coordinates, goodId))
        {
            return true;
        }

        return TryMovingIntoInventory(belt, dst, goodId);
    }

    public bool TryGrabContentIntoBelt(ConveyorBeltComponent belt)
    {
        var conn = belt.GetComponent<ConveyorConnection>();
        var src = conn ? conn.NeighborAt(belt.InputCoordinates) : null;
        if (!src) { return false; }

        return TryGrabFromInventory(belt, src!);
    }

    public string GetGoodType(string goodId) => goods.GetGood(goodId).GoodType;

    public IEnumerable<GoodSpec> GetQualifiedGoods(ConveyorBeltComponent belt)
    {
        var list = belt.Spec.ForbiddenGoodTypes;
        var listEmpty = list.Length == 0;

        foreach (var id in goods.Goods)
        {
            var g = goods.GetGood(id);

            if (listEmpty || !list.Contains(g.GoodType))
            {
                yield return g;
            }
        }
    }

    bool TryMovingIntoBelt(ConveyorBeltComponent src, ConveyorBeltComponent dst, Vector3Int srcCoords, string goodId)
    {
        if (!dst.CanAcceptItem(goodId)) { return false; }
        if (!dst.IsInputCoordinates(srcCoords)) { return false; }

        var item = src.Pop();
        dst.Push(item.GoodId);
        return true;
    }

    bool TryMovingIntoInventory(ConveyorBeltComponent src, ConveyorConnection conn, string goodId)
    {
        foreach (var inv in conn.GetUsableInventories())
        {
            if (!inv.IsInput) { continue; }
            if (!inv.HasUnreservedCapacity(goodId)) { continue; }

            var item = src.Pop();
            inv.GiveExisting(new(item.GoodId, 1));
            return true;
        }

        return false;
    }

    bool TryGrabFromInventory(ConveyorBeltComponent belt, ConveyorConnection conn)
    {
        foreach (var inv in conn.GetUsableInventories())
        {
            if (!inv.IsOutput) { continue; }

            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount == 0) { continue; }
                if (!belt.IsValidGood(stock.GoodId)) { continue; }

                belt.Push(stock.GoodId);
                inv.TakeExisting(new(stock.GoodId, 1));
                return true;
            }
        }

        return false;
    }
}
