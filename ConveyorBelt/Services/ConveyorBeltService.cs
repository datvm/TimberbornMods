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
    readonly Dictionary<Vector3Int, ConveyorBeltJunction> junctions = [];

    public IReadOnlyDictionary<Vector3Int, ConveyorBeltComponent> Belts => belts;
    public IReadOnlyDictionary<Vector3Int, InventoryInfo> Inventories => inventories;
    public IReadOnlyDictionary<Vector3Int, ConveyorBeltJunction> Junctions => junctions;

    public void Load()
    {
        HoursPerTick = dayNightCycle.TicksToHours(1);
        eb.Register(this);
    }

    [OnEvent]
    public void OnFinished(EnteredFinishedStateEvent e) =>
        OnEnityFinishedStateChanged(e.BlockObject,
            jc => junctions[jc.Coordinates] = jc,
            (info, coords) =>
            {
                if (!inventories.ContainsKey(coords))
                {
                    inventories[coords] = info;
                }
            });

    [OnEvent]
    public void OnDestroyed(ExitedFinishedStateEvent e) => RemoveObject(e.BlockObject);

    void RemoveObject(BlockObject? comp)
    {
        if (!comp) { return; }

        OnEnityFinishedStateChanged(comp!,
            jc => junctions.Remove(jc.Coordinates),
            (info, coords) =>
            {
                if (inventories.TryGetValue(coords, out var existing) && existing == info)
                {
                    inventories.Remove(coords);
                }
            });
    }

    void OnEnityFinishedStateChanged(BlockObject e, Action<ConveyorBeltJunction> jcAction, Action<InventoryInfo, Vector3Int> inventoryAction)
    {
        var jc = e.GetComponent<ConveyorBeltJunction>();
        if (jc)
        {
            jcAction(jc);
            return;
        }

        if (!e.HasComponent<BuildingSpec>()) { return; } // Only work on buildings

        if (InventoryFinder.LookForInventories(e) is not InventoryInfo info
            || info.Type == InventoryType.None) { return; }
        foreach (var coords in GetOccupiedCoordinates(e))
        {
            inventoryAction(info, coords);
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
        if (belts.TryGetValue(outputCoord, out var nextBelt) && TryMovingIntoBelt(belt, nextBelt, coords, goodId))
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
        if (!inv) {  return false; }

        foreach (var stock in inv.UnreservedTakeableStock())
        {
            if (stock.Amount == 0) { continue; }
            if (!belt.IsValidGood(stock.GoodId)) { continue; }

            belt.Push(stock.GoodId);
            inv.TakeExisting(new(stock.GoodId, 1));
            return true;
        }

        return false;
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

    static IEnumerable<Vector3Int> GetOccupiedCoordinates(BlockObject bo)
        => bo.PositionedBlocks.GetAllCoordinates();

    bool TryMovingIntoBelt(ConveyorBeltComponent src, ConveyorBeltComponent dst, Vector3Int srcCoords, string goodId)
    {
        if (!dst.CanAcceptItem(goodId)) { return false; }
        if (!dst.IsInputCoordinates(srcCoords)) { return false; }

        var item = src.Pop();
        dst.Push(item.GoodId);
        return true;
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

}
