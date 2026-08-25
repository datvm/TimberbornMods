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

    public IReadOnlyDictionary<string, int> Requested => requested;
    public IReadOnlyDictionary<string, int> Stock => stock;
    public int WeightLimit => trebuchet.Spec.WeightLimit;
    public int PayloadWeight => WeightOf(requested);
    public bool IsReady => HasPayload && HasLaunchCost();

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
        => trebuchet.IsFinished
            && trebuchet.Mode != TrebuchetLaunchMode.None
            && HasPayload
            && head.Crane == crane;

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

            Notify(listing: true);
            return true;
        }

        var previous = requested.GetValueOrDefault(goodId);
        requested[goodId] = amount;
        if (PayloadWeight > WeightLimit)
        {
            if (previous <= 0)
            {
                requested.Remove(goodId);
            }
            else
            {
                requested[goodId] = previous;
            }

            return false;
        }

        Notify(listing: previous <= 0, materials: true);
        return true;
    }

    public int MaxAmountFor(string goodId)
    {
        var weight = Math.Max(1, goods.GetGood(goodId).Weight);
        var used = PayloadWeight - requested.GetValueOrDefault(goodId) * weight;
        return Math.Max(0, (WeightLimit - used) / weight);
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

    bool HasLaunchCost()
    {
        foreach (var cost in trebuchet.Spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            if (stock.GetValueOrDefault(cost.Id) < cost.Amount)
            {
                return false;
            }
        }

        foreach (var (id, amount) in requested)
        {
            if (stock.GetValueOrDefault(id) < amount)
            {
                return false;
            }
        }

        return true;
    }

    IEnumerable<GoodAmount> Needed()
    {
        Dictionary<string, int> needed = [];
        foreach (var (id, amount) in requested)
        {
            needed[id] = amount;
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
        }

        if (listing)
        {
            RefreshListing();
        }
    }

    void RefreshListing()
    {
        if (IsForCrane(head.Crane ?? null!))
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
