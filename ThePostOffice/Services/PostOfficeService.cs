namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeService(
    PostOfficeGlobalInventoryService inventory,
    PostOfficeConfigService configs,
    RecoveredGoodStackSpawner spawner,
    EventBus events
)
{
    public int ParcelWeight => configs.ParcelWeight;

    public int ItemWeight(string id)
    {
        if (!inventory.Inventory.ContainsKey(id))
        {
            return 0;
        }

        return inventory.GetItemWeight(id);
    }

    public int GoodsWeight(IEnumerable<GoodAmount> goods)
    {
        var sum = 0;
        foreach (var good in goods)
        {
            var unit = ItemWeight(good.GoodId);
            if (unit <= 0 || good.Amount <= 0)
            {
                continue;
            }

            sum += unit * good.Amount;
        }

        return sum;
    }

    public int GlobalAmount(string id)
    {
        if (!inventory.Inventory.TryGetValue(id, out var good))
        {
            return 0;
        }

        return good.Amount;
    }

    public int OutgoingAmount(string id) => inventory.OutgoingAmount(id);

    public int Available(string id) => inventory.Available(id);

    public int StockWeight(Inventory stock)
    {
        var sum = 0;
        foreach (var good in stock.Stock)
        {
            sum += ItemWeight(good.GoodId) * good.Amount;
        }

        return sum;
    }

    public int? TryReserveIncoming(IReadOnlyList<GoodAmount> goods) => inventory.TryReserveIncoming(goods);

    public int? TryReserveOutgoing(IReadOnlyList<GoodAmount> goods) => inventory.TryReserveOutgoing(goods);

    public void Release(int id) => inventory.Release(id);

    public void Commit(int id) => inventory.Commit(id);

    public void Spawn(Vector3Int coordinates, IReadOnlyList<GoodAmount> goods)
    {
        if (goods.Count == 0)
        {
            return;
        }

        spawner.AddAwaitingGoods(coordinates, goods);
    }

    public List<PostOfficeParcel> Split(IReadOnlyList<GoodAmount> goods)
    {
        List<PostOfficeParcel> result = [];
        List<GoodAmount> current = [];
        var budget = ParcelWeight;

        void Flush()
        {
            if (current.Count == 0)
            {
                return;
            }

            result.Add(new([..current], false));
            current = [];
            budget = ParcelWeight;
        }

        foreach (var good in goods)
        {
            var unit = ItemWeight(good.GoodId);
            if (good.Amount <= 0 || unit <= 0)
            {
                continue;
            }

            if (unit > ParcelWeight)
            {
                Flush();
                for (var i = 0; i < good.Amount; i++)
                {
                    result.Add(new([new GoodAmount(good.GoodId, 1)], false));
                }

                continue;
            }

            var left = good.Amount;
            while (left > 0)
            {
                if (budget < unit)
                {
                    Flush();
                }

                var fit = Math.Min(left, budget / unit);
                if (fit <= 0)
                {
                    Flush();
                    continue;
                }

                current.Add(new(good.GoodId, fit));
                budget -= fit * unit;
                left -= fit;
            }
        }

        Flush();
        return result;
    }

    public PostOfficeParcel? PackOne(IReadOnlyList<GoodAmount> needs, bool auto)
    {
        List<GoodAmount> goods = [];
        var budget = ParcelWeight;
        foreach (var good in needs)
        {
            var unit = ItemWeight(good.GoodId);
            if (good.Amount <= 0 || unit <= 0)
            {
                continue;
            }

            if (unit > ParcelWeight)
            {
                if (goods.Count > 0)
                {
                    continue;
                }

                goods.Add(new(good.GoodId, 1));
                break;
            }

            if (budget < unit)
            {
                continue;
            }

            var fit = Math.Min(good.Amount, budget / unit);
            if (fit <= 0)
            {
                continue;
            }

            goods.Add(new(good.GoodId, fit));
            budget -= fit * unit;
            if (budget <= 0)
            {
                break;
            }
        }

        if (goods.Count == 0)
        {
            return null;
        }

        return new([..goods], auto);
    }

    public void AddContribution(PostOfficeCapacity building)
    {
        inventory.AddCapacity(building.Contribution);
    }

    public void RemoveContribution(PostOfficeCapacity building)
    {
        var removing = Math.Min(building.Contribution, inventory.Capacity);
        var goods = inventory.GetGoodRemoval(removing).ToArray();
        if (goods.Length > 0)
        {
            inventory.Remove(goods);
            spawner.AddAwaitingGoods(building.Coordinates, goods);
        }

        inventory.RemoveCapacity(removing);
        var dropped = inventory.DropHoldsThatDoNotFit();
        if (dropped.Length > 0)
        {
            events.Post(new OnPostOfficeHoldsReleased(dropped));
        }
    }
}
