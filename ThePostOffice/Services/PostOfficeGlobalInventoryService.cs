namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeGlobalInventoryService(
    GlobalInventoryService globalInventory,
    IGoodService goods,
    EventBus eb
) : ILoadableSingleton
{
    public IReadOnlyDictionary<string, PostOfficeGood> Reserved { get; private set; } = null!;
    public IReadOnlyDictionary<string, PostOfficeInventoryGood> Inventory { get; private set; } = null!;

    public int TotalWeight { get; private set; }
    public int ReservedWeight { get; private set; }
    public int Capacity { get; private set; }
    public int RemainingCapacity => Capacity - TotalWeight - ReservedWeight;

    string? changingId;

    public void Load()
    {
        var inv = new Dictionary<string, PostOfficeInventoryGood>();
        var res = new Dictionary<string, PostOfficeGood>();

        foreach (var id in goods.Goods)
        {
            var spec = goods.GetGood(id);
            var handle = globalInventory.Get(id);

            inv[id] = new(spec, handle);
            res[id] = new(spec);
        }

        Inventory = inv;
        Reserved = res;
        RefreshWeight();

        eb.Register(this);
    }

    public void RefreshWeight()
    {
        TotalWeight = Inventory.Values.Sum(g => g.TotalWeight);
        ReservedWeight = Reserved.Values.Sum(r => r.TotalWeight);
    }

    public bool TryReserve(IReadOnlyList<GoodAmount> goods)
    {
        var weight = GetItemWeight(goods);
        if (weight > RemainingCapacity)
        {
            return false;
        }

        ReservedWeight += weight;
        foreach (var g in goods)
        {
            Reserved[g.GoodId].Amount += g.Amount;
        }

        return true;
    }

    public void Unreserve(IReadOnlyList<GoodAmount> goods)
    {
        foreach (var g in goods)
        {
            var curr = Reserved[g.GoodId].Amount;
            if (curr < g.Amount)
            {
                throw new InvalidOperationException($"Cannot unreserve {g.Amount} of {g.GoodId}, only {curr} reserved.");
            }
        }

        var weight = 0;
        foreach (var g in goods)
        {
            Reserved[g.GoodId].Amount -= g.Amount;
            weight += GetItemWeight(g.GoodId, g.Amount);
        }

        ReservedWeight -= weight;
    }

    public void AddReserved(IReadOnlyList<GoodAmount> goods)
    {
        Unreserve(goods);
        AddToInventory(goods);
    }

    public void AddWithoutReserve(IReadOnlyList<GoodAmount> goods)
    {
        var weight = GetItemWeight(goods);
        if (weight > RemainingCapacity)
        {
            throw new InvalidOperationException($"Cannot add {weight} weight of goods, only {RemainingCapacity} remaining capacity.");
        }

        AddToInventory(goods);
    }

    public void Remove(IReadOnlyList<GoodAmount> goods)
    {
        foreach (var g in goods)
        {
            var have = Inventory[g.GoodId].Amount;
            if (have < g.Amount)
            {
                throw new InvalidOperationException($"Cannot remove {g.Amount} of {g.GoodId}, only {have} stored.");
            }
        }

        var weight = 0;
        foreach (var g in goods)
        {
            var poGood = Inventory[g.GoodId];
            InternalChangeInventory(poGood.GlobalGoodHandle, -g.Amount);
            weight += poGood.ItemWeight * g.Amount;
        }

        TotalWeight -= weight;
    }

    void AddToInventory(IReadOnlyList<GoodAmount> goods)
    {
        var weight = 0;
        foreach (var g in goods)
        {
            var poGood = Inventory[g.GoodId];
            InternalChangeInventory(poGood.GlobalGoodHandle, g.Amount);
            weight += poGood.ItemWeight * g.Amount;
        }

        TotalWeight += weight;
    }

    public IEnumerable<GoodAmount> GetGoodRemoval(int removingCap)
    {
        RefreshWeight();
        var targetCap = Capacity - removingCap;
        if (targetCap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(removingCap), "Target capacity must be non-negative.");
        }

        var remaining = TotalWeight - targetCap;
        if (remaining <= 0)
        {
            yield break;
        }

        var goodsByWeight = Inventory.Values
            .Where(g => g.Amount > 0 && g.ItemWeight > 0)
            .OrderByDescending(g => g.ItemWeight);

        foreach (var g in goodsByWeight)
        {
            var w = g.ItemWeight;
            var removing = Math.Min(g.Amount, Mathf.CeilToInt((float)remaining / w));

            if (removing > 0)
            {
                yield return new GoodAmount(g.GlobalGoodHandle.Spec.Id, removing);
                remaining -= removing * w;

                if (remaining <= 0)
                {
                    yield break;
                }
            }
        }

        // Should not reach here
    }

    public void AddCapacity(int cap)
    {
        Capacity += cap;
        eb.Post(new OnPostOfficeCapacityChanged(Capacity));
    }

    public void RemoveCapacity(int cap)
    {
        RefreshWeight();

        var target = Capacity - cap;
        if (target < TotalWeight)
        {
            throw new InvalidOperationException($"Cannot remove {cap} capacity, only {Capacity} available and {TotalWeight} weight in inventory.");
        }

        Capacity = target;
        eb.Post(new OnPostOfficeCapacityChanged(Capacity));
    }

    void InternalChangeInventory(GlobalGoodHandle handle, int delta)
    {
        var id = changingId = handle.Spec.Id;

        var prev = (int)handle.Amount;
        handle.Add(delta);
        changingId = null;

        var curr = (int)handle.Amount;
        RaiseItemChanged(id, curr, prev);
    }

    [OnEvent]
    public void OnGlobalInventoryChanged(GlobalGoodAmountChangedEvent e)
    {
        var id = e.Handle.Spec.Id;
        if (changingId == id || !Inventory.ContainsKey(id)) { return; }

        RefreshWeight();
        RaiseItemChanged(id, (int)e.Current, (int)e.Previous);
    }

    void RaiseItemChanged(string id, int amount, int prevAmount)
        => eb.Post(new OnPostOfficeGlobalInventoryChanged(id, amount, prevAmount));

    public int GetItemWeight(string id) => Inventory[id].ItemWeight;
    public int GetItemWeight(string id, int amount) => GetItemWeight(id) * amount;
    public int GetItemWeight(IEnumerable<GoodAmount> goods) => goods.Sum(g => GetItemWeight(g.GoodId, g.Amount));
    public int GetTotalWeight(string id) => Inventory[id].TotalWeight;

}

public readonly record struct OnPostOfficeGlobalInventoryChanged(string Id, int Amount, int PrevAmount);

public readonly record struct OnPostOfficeCapacityChanged(int Capacity);