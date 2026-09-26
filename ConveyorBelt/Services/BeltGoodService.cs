namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltGoodService(IGoodService goods, RecoveredGoodStackSpawner spawner) : ILoadableSingleton
{
    public const string Liquid = "Liquid";

    readonly Dictionary<string, string> types = [];
    readonly List<GoodAmount> drop = [];

    public void Load()
    {
        foreach (var id in goods.Goods)
        {
            types[id] = goods.GetGood(id).GoodType;
        }
    }

    public bool IsCarryable(string goodId) => TypeOf(goodId) != Liquid;

    public string TypeOf(string goodId)
    {
        if (types.TryGetValue(goodId, out var type))
        {
            return type;
        }

        type = goods.GetGood(goodId).GoodType;
        types[goodId] = type;
        return type;
    }

    public IEnumerable<GoodSpec> CarryableGoods()
    {
        foreach (var id in goods.Goods)
        {
            var good = goods.GetGood(id);
            if (good.GoodType == Liquid)
            {
                continue;
            }

            yield return good;
        }
    }

    public bool TryTake(BlockObject block, Inventories inventories, out string goodId)
    {
        goodId = "";
        if (!inventories)
        {
            return false;
        }

        foreach (var inv in inventories.EnabledInventories)
        {
            if (!inv || !inv.IsOutput || IsIgnored(block, inv))
            {
                continue;
            }

            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount < 1 || !IsCarryable(stock.GoodId))
                {
                    continue;
                }

                inv.TakeExisting(new(stock.GoodId, 1));
                goodId = stock.GoodId;
                return true;
            }
        }

        return false;
    }

    public bool TryGive(BlockObject block, Inventories inventories, string goodId)
    {
        if (!inventories || !IsCarryable(goodId))
        {
            return false;
        }

        var packet = new GoodAmount(goodId, 1);
        foreach (var inv in inventories.EnabledInventories)
        {
            if (!inv || !inv.IsInput || IsIgnored(block, inv))
            {
                continue;
            }

            if (!inv.Takes(goodId) || !inv.HasUnreservedCapacity(packet))
            {
                continue;
            }

            inv.GiveExisting(packet);
            return true;
        }

        return false;
    }

    public void Drop(Vector3Int cell, List<BeltGood> items)
    {
        drop.Clear();
        foreach (var item in items)
        {
            var found = false;
            for (var i = 0; i < drop.Count; i++)
            {
                if (drop[i].GoodId != item.Id)
                {
                    continue;
                }

                drop[i] = new(item.Id, drop[i].Amount + 1);
                found = true;
                break;
            }

            if (!found)
            {
                drop.Add(new(item.Id, 1));
            }
        }

        if (drop.Count > 0)
        {
            spawner.AddAwaitingGoods(cell, drop);
        }
    }

    static bool IsIgnored(BlockObject block, Inventory inventory)
    {
        if (block.GetComponentOrNull<ConstructionSite>() is { } site && site.Inventory == inventory)
        {
            return true;
        }

        return block.GetComponentOrNull<RecoveredGoodStack>() is { } recovered && recovered.Inventory == inventory;
    }
}
