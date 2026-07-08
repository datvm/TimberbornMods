namespace ConveyorBelt.Services;

[BindSingleton]
public class ConveyorBeltService(
    RecoveredGoodStackSpawner goodStackSpawner,
    IDayNightCycle dayNightCycle,
    ILoc t,
    EventBus eb,
    IGoodService goods
) : ILoadableSingleton
{
    public readonly ILoc t = t;

    public float HoursPerTick { get; private set; }
    readonly Dictionary<Vector3Int, ConveyorBeltComponent> belts = [];
    readonly Dictionary<Vector3Int, InventoryInfo> inventories = [];

    public void Load()
    {        
        HoursPerTick = dayNightCycle.TicksToHours(1);
        eb.Register(this);
    }

    [OnEvent]
    public void OnFinished(EnteredFinishedStateEvent e)
    {
        if (InventoryFinder.LookForInventories(e.BlockObject) is not InventoryInfo info
            || info.Type == InventoryType.None) { return; }

        foreach (var coords in GetOccupiedCoordinates(e.BlockObject))
        {
            inventories[coords] = info;
        }
    }

    [OnEvent]
    public void OnDestroyed(ExitedFinishedStateEvent e)
    {
        if (InventoryFinder.LookForInventories(e.BlockObject) is not InventoryInfo info
            || info.Type == InventoryType.None) { return; }

        foreach (var coords in GetOccupiedCoordinates(e.BlockObject))
        {
            if (inventories.TryGetValue(coords, out var existing) && existing == info)
            {
                inventories.Remove(coords);
            }
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
        var coords = belt.Coordinates;
        var outputCoord = belt.OutputCoordinates;
        if (belts.TryGetValue(outputCoord, out var nextBelt) && TryMovingIntoBelt(belt, nextBelt, coords))
        {
            return true;
        }

        if (inventories.TryGetValue(outputCoord, out var inv) && TryMovingIntoInventory(belt, inv, goodId))
        {
            return true;
        }

        return false;
    }

    public bool TryGrabContentIntoBelt(ConveyorBeltComponent belt)
    {
        var coords = belt.InputCoordinates;
        if (!inventories.TryGetValue(coords, out var info)) { return false; }
        if ((info.Type & InventoryType.Out) == 0) { return false; }

        var inv = info.Inventory;

        foreach (var stock in inv.UnreservedTakeableStock())
        {
            if (stock.Amount == 0) { continue; }
            if (!CanCarry(belt, stock.GoodId)) { continue; }

            belt.Push(stock.GoodId);
            inv.TakeExisting(new(stock.GoodId, 1));
            return true;
        }

        return false;
    }

    static IEnumerable<Vector3Int> GetOccupiedCoordinates(BlockObject bo)
        => bo.PositionedBlocks.GetAllCoordinates();

    bool TryMovingIntoBelt(ConveyorBeltComponent src, ConveyorBeltComponent dst, Vector3Int srcCoords)
    {
        if (!dst.CanAcceptItem) { return false; }
        if (!dst.IsInputCoordinates(srcCoords)) { return false; }
        if (!CanCarry(dst, src.Head!.GoodId)) { return false; }

        MoveItemBetweenBelts(src, dst);
        return true;
    }

    static void MoveItemBetweenBelts(ConveyorBeltComponent src, ConveyorBeltComponent dst)
    {
        var item = src.Pop();
        dst.Push(item.GoodId);
    }

    bool TryMovingIntoInventory(ConveyorBeltComponent src, InventoryInfo info, string goodId)
    {
        if ((info.Type & InventoryType.In) == 0) { return false; }

        var inv = info.Inventory;
        if (inv.UnreservedCapacity(goodId) <= 0) { return false; }

        var item = src.Pop();
        inv.GiveExisting(new(item.GoodId, 1));
        return true;
    }

    bool CanCarry(ConveyorBeltComponent belt, string goodId)
    {
        var forbiddenTypes = belt.Spec.ForbiddenGoodTypes;
        if (forbiddenTypes.Length == 0) { return true; }

        var good = goods.GetGood(goodId);
        return !forbiddenTypes.Contains(good.GoodType);
    }

}
