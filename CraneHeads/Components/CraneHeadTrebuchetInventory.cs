namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetInventory(
    IGoodService goods,
    TrebuchetLaunchJobProvider provider
) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IFinishedStateListener, IPersistentEntity, IMaterialCraneJob
{
    static readonly ComponentKey SaveKey = new(nameof(CraneHeadTrebuchetInventory));
    static readonly PropertyKey<string> RequestedKey = new("Requested");
    static readonly PropertyKey<string> StockKey = new("Stock");
    static readonly PropertyKey<int> PriorityKey = new("Priority");

    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadComponent head = null!;
    readonly Dictionary<string, int> requested = [];
    readonly Dictionary<string, int> stock = [];
    bool listed;
    bool wasReady;
    bool wasOverweight;

    public IReadOnlyDictionary<string, int> Requested => requested;
    public IReadOnlyDictionary<string, int> Stock => stock;
    public int AmountInStock(string goodId) => stock.GetValueOrDefault(goodId);

    public IEnumerable<GoodAmount> PayloadNeed()
    {
        foreach (var (id, amount) in requested)
        {
            yield return new(id, amount);
        }
    }

    public IEnumerable<GoodAmount> LaunchCostNeed()
    {
        var capacity = Math.Max(1, trebuchet.Spec.LaunchCostCapacity);
        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            yield return new(cost.Id, cost.Amount * capacity);
        }
    }

    public int LaunchCostPerShot(string goodId)
    {
        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (cost.Id == goodId)
            {
                return cost.Amount;
            }
        }

        return 0;
    }

    public int WeightLimit => trebuchet.Spec.WeightLimit;
    public int PayloadWeight => WeightOf(requested);
    public bool IsOverweight => PayloadWeight > WeightLimit;
    public bool IsReady
    {
        get
        {
            if (!HasPayload || IsOverweight)
            {
                return false;
            }

            foreach (var amount in LaunchNeed())
            {
                if (stock.GetValueOrDefault(amount.GoodId) < amount.Amount)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public Priority Priority { get; private set; } = Priority.Normal;
    public string JobNameLoc => "LV.CrH.JobLaunch";
    public bool IsAvailable => false;
    public float Progress
    {
        get
        {
            var total = 0;
            var have = 0;
            foreach (var amount in Needed())
            {
                total += amount.Amount;
                have += Math.Min(stock.GetValueOrDefault(amount.GoodId), amount.Amount);
            }

            return total <= 0 ? 0f : Mathf.Clamp01(have / (float)total);
        }
    }

    public event EventHandler? AvailabilityChanged;
    public event EventHandler<PriorityChangedEventArgs>? PriorityChanged;
    public event EventHandler? MaterialsChanged;
    public event EventHandler? Changed;
    public event EventHandler? ReadyChanged;

    bool HasPayload => requested.Count > 0;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        head = GetComponent<CraneHeadComponent>();
    }

    public void InitializeEntity()
    {
        trebuchet.ModeChanged += OnModeChanged;
        head.CraneChanged += OnCraneChanged;
        wasReady = IsReady;
        wasOverweight = IsOverweight;
        RefreshListing();
    }

    public void DeleteEntity()
    {
        trebuchet.ModeChanged -= OnModeChanged;
        head.CraneChanged -= OnCraneChanged;
        Unlist();
    }

    public void OnEnterFinishedState() => RefreshListing();

    public void OnExitFinishedState() => Unlist();

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(RequestedKey, Serialize(requested));
        s.Set(StockKey, Serialize(stock));
        s.Set(PriorityKey, (int)Priority);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(RequestedKey))
        {
            Deserialize(s.Get(RequestedKey), requested);
        }

        if (s.Has(StockKey))
        {
            Deserialize(s.Get(StockKey), stock);
        }

        if (s.Has(PriorityKey))
        {
            Priority = (Priority)s.Get(PriorityKey);
        }
    }

    public bool IsForCrane(CraneComponent crane)
        => trebuchet.IsFinished && head.Crane == crane;

    public IEnumerable<GoodAmount> GetRemainingMaterials()
    {
        foreach (var amount in Needed())
        {
            var left = amount.Amount - stock.GetValueOrDefault(amount.GoodId);
            if (left > 0)
            {
                yield return new(amount.GoodId, left);
            }
        }
    }

    public IEnumerable<GoodAmount> GetTotalMaterials() => Needed();

    public int AddMaterial(GoodAmount material)
    {
        var remaining = 0;
        foreach (var amount in Needed())
        {
            if (amount.GoodId == material.GoodId)
            {
                remaining += amount.Amount;
            }
        }

        var take = Math.Min(material.Amount, Math.Max(0, remaining - stock.GetValueOrDefault(material.GoodId)));
        if (take <= 0)
        {
            return 0;
        }

        stock[material.GoodId] = stock.GetValueOrDefault(material.GoodId) + take;
        Notify(materials: true);
        return take;
    }

    public void ProgressJob(CraneComponent crane, float hours) { }

    public void SetPriority(Priority priority)
    {
        if (Priority == priority)
        {
            return;
        }

        var previous = Priority;
        Priority = priority;
        PriorityChanged?.Invoke(this, new(previous));
    }

    public bool TrySetGood(string goodId, int amount)
    {
        if (string.IsNullOrEmpty(goodId))
        {
            return false;
        }

        if (amount <= 0)
        {
            if (!requested.Remove(goodId))
            {
                return false;
            }

            ReturnSurplusToCrane();
            Notify(listing: true, materials: true);
            return true;
        }

        var previous = requested.GetValueOrDefault(goodId);
        requested[goodId] = Math.Clamp(amount, 1, 99);
        ReturnSurplusToCrane();
        Notify(listing: previous <= 0, materials: true);
        return true;
    }

    public int MaxAmountFor(string goodId)
    {
        if (string.IsNullOrEmpty(goodId) || requested.ContainsKey(goodId))
        {
            return 0;
        }

        return 99;
    }

    public bool TryRemoveForLaunch(List<GoodAmount> payload)
    {
        payload.Clear();
        if (!IsReady)
        {
            return false;
        }

        foreach (var (id, amount) in requested)
        {
            stock[id] = stock.GetValueOrDefault(id) - amount;
            if (stock[id] <= 0)
            {
                stock.Remove(id);
            }

            payload.Add(new(id, amount));
        }

        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            stock[cost.Id] = stock.GetValueOrDefault(cost.Id) - cost.Amount;
            if (stock[cost.Id] <= 0)
            {
                stock.Remove(cost.Id);
            }
        }

        Notify(materials: true);
        return true;
    }

    IEnumerable<GoodAmount> LaunchNeed()
    {
        Dictionary<string, int> needed = [];
        foreach (var (id, amount) in requested)
        {
            needed[id] = amount;
        }

        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            needed[cost.Id] = needed.GetValueOrDefault(cost.Id) + cost.Amount;
        }

        foreach (var (id, amount) in needed)
        {
            yield return new(id, amount);
        }
    }

    IEnumerable<GoodAmount> Needed()
    {
        Dictionary<string, int> needed = [];
        if (!IsOverweight)
        {
            foreach (var (id, amount) in requested)
            {
                needed[id] = amount;
            }
        }

        var capacity = Math.Max(1, trebuchet.Spec.LaunchCostCapacity);
        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            needed[cost.Id] = needed.GetValueOrDefault(cost.Id) + cost.Amount * capacity;
        }

        foreach (var (id, amount) in needed)
        {
            yield return new(id, amount);
        }
    }

    int WeightOf(Dictionary<string, int> amounts)
    {
        var weight = 0;
        foreach (var (id, amount) in amounts)
        {
            weight += amount * Math.Max(1, goods.GetGood(id).Weight);
        }

        return weight;
    }

    void ReturnSurplusToCrane()
    {
        Dictionary<string, int> keep = [];
        foreach (var (id, amount) in requested)
        {
            keep[id] = amount;
        }

        var capacity = Math.Max(1, trebuchet.Spec.LaunchCostCapacity);
        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            keep[cost.Id] = keep.GetValueOrDefault(cost.Id) + cost.Amount * capacity;
        }

        foreach (var id in stock.Keys.ToArray())
        {
            var extra = stock[id] - keep.GetValueOrDefault(id);
            if (extra <= 0)
            {
                continue;
            }

            ReturnToCrane(id, extra);
        }
    }

    void ReturnToCrane(string goodId, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (head.GetCrane() is not { } crane)
        {
            return;
        }

        var craneInventory = crane.GetComponent<CraneInventory>();
        if (!craneInventory || !craneInventory.Inventory)
        {
            return;
        }

        craneInventory.Inventory.GiveExistingIgnoringCapacity(new(goodId, amount));
        var left = stock.GetValueOrDefault(goodId) - amount;
        if (left <= 0)
        {
            stock.Remove(goodId);
            return;
        }

        stock[goodId] = left;
    }

    void OnModeChanged(object sender, EventArgs e) => RefreshListing();

    void OnCraneChanged(object sender, EventArgs e) => RefreshListing();

    void Notify(bool listing = false, bool materials = false)
    {
        Changed?.Invoke(this, EventArgs.Empty);
        if (materials)
        {
            MaterialsChanged?.Invoke(this, EventArgs.Empty);
        }

        var ready = IsReady;
        if (ready != wasReady)
        {
            wasReady = ready;
            ReadyChanged?.Invoke(this, EventArgs.Empty);
            AvailabilityChanged?.Invoke(this, EventArgs.Empty);
        }

        var overweight = IsOverweight;
        if (listing || overweight != wasOverweight)
        {
            wasOverweight = overweight;
            RefreshListing();
        }
    }

    void RefreshListing()
    {
        if (head.Crane is { } crane && IsForCrane(crane))
        {
            ListJob();
            return;
        }

        Unlist();
    }

    void ListJob()
    {
        if (listed)
        {
            return;
        }

        listed = true;
        provider.NotifyNew(this);
    }

    void Unlist()
    {
        if (!listed)
        {
            return;
        }

        listed = false;
        provider.NotifyRemoved(this);
    }

    static string Serialize(Dictionary<string, int> amounts)
    {
        List<string> parts = [];
        foreach (var (id, amount) in amounts)
        {
            if (amount <= 0)
            {
                continue;
            }

            parts.Add(id);
            parts.Add(amount.ToString());
        }

        return string.Join(";", parts);
    }

    static void Deserialize(string raw, Dictionary<string, int> amounts)
    {
        amounts.Clear();
        if (string.IsNullOrEmpty(raw))
        {
            return;
        }

        var parts = raw.Split(';');
        for (var i = 0; i + 1 < parts.Length; i += 2)
        {
            var id = parts[i];
            if (string.IsNullOrEmpty(id) || !int.TryParse(parts[i + 1], out var amount) || amount <= 0)
            {
                continue;
            }

            amounts[id] = amount;
        }
    }
}
