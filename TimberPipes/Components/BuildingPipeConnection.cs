namespace TimberPipes.Components;

public interface IBuildingPipeConnection
{
    bool Give { get; }
    bool Take => !Give;

    BuildingPipeTarget Target { get; }

    bool IsValid { get; }

    bool IsAttached { get; }

    IEnumerable<string> GetLiquidIds();
    LiquidInventory GetLiquidInventory(string id);

    bool TryTransfer(string goodId, int amount);
}

public class DefaultBuildingPipeConnection(
    BuildingPipeTarget building,
    ValvePipeService service,
    bool give) : IBuildingPipeConnection
{
    public bool Give => give;

    public BuildingPipeTarget Target => building;

    public bool IsAttached => building && building.BlockObject;

    public bool IsValid
        => IsAttached
            && building.BlockObject.IsFinished
            && ValvePipeIo.HasActiveInventory(EnabledOperationalCount());

    public IEnumerable<string> GetLiquidIds()
        => ValvePipeIo.LiquidIdsForConnection(InventorySources(), service.LiquidIds, give);

    public LiquidInventory GetLiquidInventory(string id)
    {
        var current = 0;
        var max = 0;
        foreach (var inv in EnabledOperationalInventories())
        {
            current += inv.AmountInStock(id);
            max += inv.LimitedAmount(id);
        }

        return new(current, max);
    }

    public bool TryTransfer(string goodId, int amount)
        => IsValid && (give ? TryGive(goodId, amount) : TryTake(goodId, amount));

    bool TryGive(string goodId, int amount)
    {
        if (amount < 1)
        {
            return false;
        }

        var packet = new GoodAmount(goodId, amount);
        foreach (var inv in EnabledOperationalInventories())
        {
            if (!ValvePipeIo.CanGiveToBuilding(inv.Takes(goodId), inv.HasUnreservedCapacity(packet)))
            {
                continue;
            }

            inv.GiveExisting(packet);
            return true;
        }

        return false;
    }

    bool TryTake(string goodId, int amount)
    {
        if (amount < 1)
        {
            return false;
        }

        var packet = new GoodAmount(goodId, amount);
        foreach (var inv in EnabledOperationalInventories())
        {
            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.GoodId != goodId || !ValvePipeIo.CanTakeFromBuilding(stock.Amount))
                {
                    continue;
                }

                if (stock.Amount < amount)
                {
                    continue;
                }

                inv.TakeExisting(packet);
                return true;
            }
        }

        return false;
    }

    int EnabledOperationalCount()
    {
        var count = 0;
        foreach (var _ in EnabledOperationalInventories())
        {
            count++;
        }

        return count;
    }

    IEnumerable<InventoryLiquidSource> InventorySources()
    {
        if (!building.Inventories)
        {
            yield break;
        }

        var construction = ConstructionInventory();
        foreach (var inv in building.Inventories.AllInventories)
        {
            if (!inv)
            {
                continue;
            }

            yield return new(
                inv == construction,
                [.. inv.InputGoods],
                [.. inv.OutputGoods],
                TakeableIds(inv));
        }
    }

    IEnumerable<Inventory> EnabledOperationalInventories()
    {
        if (!building.Inventories)
        {
            yield break;
        }

        var construction = ConstructionInventory();
        foreach (var inv in building.Inventories.EnabledInventories)
        {
            if (!inv || inv == construction)
            {
                continue;
            }

            yield return inv;
        }
    }

    Inventory? ConstructionInventory()
    {
        if (building.GetComponentOrNull<ConstructionSite>() is not { } site || !site.Inventory)
        {
            return null;
        }

        return site.Inventory;
    }

    static List<string> TakeableIds(Inventory inv)
    {
        List<string> ids = [];
        foreach (var stock in inv.UnreservedTakeableStock())
        {
            if (stock.Amount > 0)
            {
                ids.Add(stock.GoodId);
            }
        }

        return ids;
    }
}
