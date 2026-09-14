namespace TimberPipes.Components;

public interface IBuildingPipeConnection
{
    bool Give { get; }
    bool Take => !Give;

    BuildingPipeTarget Target { get; }

    bool IsValid { get; }

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

    public bool IsValid
        => building.BlockObject
            && building.BlockObject.IsFinished
            && building.Inventories
            && ValvePipeIo.HasActiveInventory(building.Inventories.EnabledInventories.Count);

    public IEnumerable<string> GetLiquidIds()
        => give
            ? ValvePipeIo.KnownExtractLiquids(InputGoodIds(), [], service.LiquidIds)
            : ValvePipeIo.KnownExtractLiquids(OutputGoodIds(), TakeableGoodIds(), service.LiquidIds);

    public LiquidInventory GetLiquidInventory(string id)
    {
        if (!building.Inventories)
        {
            return default;
        }

        var current = 0;
        var max = 0;
        foreach (var inv in building.Inventories.EnabledInventories)
        {
            current += inv.AmountInStock(id);
            max += inv.LimitedAmount(id);
        }

        return new(current, max);
    }

    public bool TryTransfer(string goodId, int amount)
        => give ? TryGive(goodId, amount) : TryTake(goodId, amount);

    bool TryGive(string goodId, int amount)
    {
        if (!building.Inventories || amount < 1)
        {
            return false;
        }

        var packet = new GoodAmount(goodId, amount);
        foreach (var inv in building.Inventories.EnabledInventories)
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
        if (!building.Inventories || amount < 1)
        {
            return false;
        }

        var packet = new GoodAmount(goodId, amount);
        foreach (var inv in building.Inventories.EnabledInventories)
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

    List<string> InputGoodIds()
    {
        List<string> ids = [];
        if (!building.Inventories)
        {
            return ids;
        }

        foreach (var inv in building.Inventories.EnabledInventories)
        {
            foreach (var id in inv.InputGoods)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    List<string> OutputGoodIds()
    {
        List<string> ids = [];
        if (!building.Inventories)
        {
            return ids;
        }

        foreach (var inv in building.Inventories.EnabledInventories)
        {
            foreach (var id in inv.OutputGoods)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    List<string> TakeableGoodIds()
    {
        List<string> ids = [];
        if (!building.Inventories)
        {
            return ids;
        }

        foreach (var inv in building.Inventories.EnabledInventories)
        {
            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount > 0)
                {
                    ids.Add(stock.GoodId);
                }
            }
        }

        return ids;
    }
}
