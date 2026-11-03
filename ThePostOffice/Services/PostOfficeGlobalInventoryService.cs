namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeGlobalInventoryService(
    GlobalInventoryService globalInventory,
    IGoodService goods,
    EventBus eb
) : ILoadableSingleton
{
    readonly Dictionary<int, PostOfficeHold> holds = [];
    readonly Dictionary<string, int> outgoing = [];

    int nextHoldId = 1;
    string? changingId;

    public IReadOnlyDictionary<string, PostOfficeInventoryGood> Inventory { get; private set; } = null!;

    public int TotalWeight { get; private set; }
    public int IncomingWeight { get; private set; }
    public int Capacity { get; private set; }
    public int RemainingCapacity => Capacity - TotalWeight - IncomingWeight;

    public void Load()
    {
        var inv = new Dictionary<string, PostOfficeInventoryGood>();
        foreach (var id in goods.Goods)
        {
            var spec = goods.GetGood(id);
            inv[id] = new(spec, globalInventory.Get(id));
        }

        Inventory = inv;
        RefreshWeight();
        eb.Register(this);
    }

    public int OutgoingAmount(string id) => outgoing.GetValueOrDefault(id);

    public void CopyIncoming(Dictionary<string, int> target)
    {
        target.Clear();
        foreach (var hold in holds.Values)
        {
            if (!hold.Incoming)
            {
                continue;
            }

            foreach (var good in hold.Goods)
            {
                if (good.Amount <= 0)
                {
                    continue;
                }

                target[good.GoodId] = target.GetValueOrDefault(good.GoodId) + good.Amount;
            }
        }
    }

    public int Available(string id)
    {
        if (!Inventory.TryGetValue(id, out var good))
        {
            return 0;
        }

        return good.Amount - OutgoingAmount(id);
    }

    public int? TryReserveIncoming(IReadOnlyList<GoodAmount> goods)
    {
        var weight = GetItemWeight(goods);
        if (weight <= 0 || weight > RemainingCapacity)
        {
            return null;
        }

        var id = nextHoldId++;
        holds[id] = new(id, true, weight, [..goods]);
        IncomingWeight += weight;
        return id;
    }

    public int? TryReserveOutgoing(IReadOnlyList<GoodAmount> goods)
    {
        if (goods.Count == 0)
        {
            return null;
        }

        foreach (var good in goods)
        {
            if (good.Amount <= 0 || Available(good.GoodId) < good.Amount)
            {
                return null;
            }
        }

        var weight = GetItemWeight(goods);
        if (weight <= 0)
        {
            return null;
        }

        var id = nextHoldId++;
        holds[id] = new(id, false, weight, [..goods]);
        AddOutgoing(goods);
        return id;
    }

    public void Release(int id)
    {
        if (!holds.Remove(id, out var hold))
        {
            return;
        }

        if (hold.Incoming)
        {
            IncomingWeight -= hold.Weight;
            return;
        }

        SubtractOutgoing(hold.Goods);
    }

    public void Commit(int id)
    {
        if (!holds.Remove(id, out var hold))
        {
            return;
        }

        if (hold.Incoming)
        {
            IncomingWeight -= hold.Weight;
            AddToInventory(hold.Goods);
            return;
        }

        SubtractOutgoing(hold.Goods);
        RemoveStored(hold.Goods);
    }

    public ImmutableArray<int> DropHoldsThatDoNotFit()
    {
        List<int> dropped = [];
        while (true)
        {
            string? shortGood = null;
            foreach (var pair in outgoing)
            {
                if (!Inventory.TryGetValue(pair.Key, out var stored) || pair.Value > stored.Amount)
                {
                    shortGood = pair.Key;
                    break;
                }
            }

            if (shortGood is null)
            {
                break;
            }

            if (NewestOutgoing(shortGood) is not int outgoingId)
            {
                break;
            }

            Release(outgoingId);
            dropped.Add(outgoingId);
        }

        var room = Capacity - TotalWeight;
        while (IncomingWeight > room)
        {
            if (NewestIncoming() is not int incomingId)
            {
                break;
            }

            Release(incomingId);
            dropped.Add(incomingId);
        }

        return [..dropped];
    }

    public void RefreshWeight()
    {
        TotalWeight = Inventory.Values.Sum(good => good.TotalWeight);
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

        var ordered = Inventory.Values
            .Where(good => good.Amount > 0 && good.ItemWeight > 0)
            .OrderByDescending(good => good.ItemWeight)
            .ToArray();
        Dictionary<string, int> planned = [];

        void Take(bool reservedOnly)
        {
            foreach (var good in ordered)
            {
                if (remaining <= 0)
                {
                    return;
                }

                var id = good.GlobalGoodHandle.Spec.Id;
                var reserved = Math.Min(OutgoingAmount(id), good.Amount);
                var free = good.Amount - reserved;
                var already = planned.GetValueOrDefault(id);
                var pool = reservedOnly
                    ? Math.Max(0, reserved - Math.Max(0, already - free))
                    : Math.Max(0, free - already);
                if (pool <= 0)
                {
                    continue;
                }

                var removing = Math.Min(pool, Mathf.CeilToInt((float)remaining / good.ItemWeight));
                if (removing <= 0)
                {
                    continue;
                }

                planned[id] = already + removing;
                remaining -= removing * good.ItemWeight;
            }
        }

        Take(false);
        Take(true);
        foreach (var pair in planned)
        {
            if (pair.Value > 0)
            {
                yield return new(pair.Key, pair.Value);
            }
        }
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

    public void Remove(IReadOnlyList<GoodAmount> goods)
    {
        foreach (var good in goods)
        {
            var have = Inventory[good.GoodId].Amount;
            if (have < good.Amount)
            {
                throw new InvalidOperationException($"Cannot remove {good.Amount} of {good.GoodId}, only {have} stored.");
            }
        }

        RemoveStored(goods);
    }

    [OnEvent]
    public void OnGlobalInventoryChanged(GlobalGoodAmountChangedEvent e)
    {
        var id = e.Handle.Spec.Id;
        if (changingId == id || !Inventory.ContainsKey(id))
        {
            return;
        }

        RefreshWeight();
        RaiseItemChanged(id, (int)e.Current, (int)e.Previous);
    }

    public int GetItemWeight(string id) => Inventory[id].ItemWeight;
    public int GetItemWeight(string id, int amount) => GetItemWeight(id) * amount;
    public int GetItemWeight(IEnumerable<GoodAmount> goods) => goods.Sum(good => GetItemWeight(good.GoodId, good.Amount));

    void RemoveStored(IReadOnlyList<GoodAmount> goods)
    {
        var raised = Apply(goods, -1);
        foreach (var change in raised)
        {
            RaiseItemChanged(change.Id, change.Amount, change.Prev);
        }
    }

    void AddToInventory(IReadOnlyList<GoodAmount> goods)
    {
        var raised = Apply(goods, 1);
        foreach (var change in raised)
        {
            RaiseItemChanged(change.Id, change.Amount, change.Prev);
        }
    }

    List<InventoryChange> Apply(IReadOnlyList<GoodAmount> goods, int sign)
    {
        List<InventoryChange> raised = [];
        var weight = 0;
        foreach (var good in goods)
        {
            var stored = Inventory[good.GoodId];
            var handle = stored.GlobalGoodHandle;
            changingId = handle.Spec.Id;
            var prev = (int)handle.Amount;
            handle.Add(sign * good.Amount);
            changingId = null;
            weight += stored.ItemWeight * good.Amount;
            raised.Add(new(handle.Spec.Id, (int)handle.Amount, prev));
        }

        TotalWeight += sign * weight;
        return raised;
    }

    void RaiseItemChanged(string id, int amount, int prevAmount)
        => eb.Post(new OnPostOfficeGlobalInventoryChanged(id, amount, prevAmount));

    void AddOutgoing(IReadOnlyList<GoodAmount> goods)
    {
        foreach (var good in goods)
        {
            outgoing[good.GoodId] = outgoing.GetValueOrDefault(good.GoodId) + good.Amount;
        }
    }

    void SubtractOutgoing(IReadOnlyList<GoodAmount> goods)
    {
        foreach (var good in goods)
        {
            var left = outgoing.GetValueOrDefault(good.GoodId) - good.Amount;
            if (left <= 0)
            {
                outgoing.Remove(good.GoodId);
            }
            else
            {
                outgoing[good.GoodId] = left;
            }
        }
    }

    int? NewestOutgoing(string goodId)
    {
        int? newest = null;
        foreach (var hold in holds.Values)
        {
            if (hold.Incoming || !ContainsGood(hold.Goods, goodId))
            {
                continue;
            }

            if (newest is null || hold.Id > newest)
            {
                newest = hold.Id;
            }
        }

        return newest;
    }

    int? NewestIncoming()
    {
        int? newest = null;
        foreach (var hold in holds.Values)
        {
            if (!hold.Incoming)
            {
                continue;
            }

            if (newest is null || hold.Id > newest)
            {
                newest = hold.Id;
            }
        }

        return newest;
    }

    static bool ContainsGood(ImmutableArray<GoodAmount> goods, string id)
    {
        foreach (var good in goods)
        {
            if (good.GoodId == id)
            {
                return true;
            }
        }

        return false;
    }
}

readonly record struct InventoryChange(string Id, int Amount, int Prev);

public readonly record struct OnPostOfficeGlobalInventoryChanged(string Id, int Amount, int PrevAmount);

public readonly record struct OnPostOfficeCapacityChanged(int Capacity);

public readonly record struct OnPostOfficeHoldsReleased(ImmutableArray<int> Ids);
