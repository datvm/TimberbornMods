namespace BuildingRenovations.Components;

[AddTemplateModule2(typeof(BuildingRenovationComponent))]
public class RenovationDistroReceiver(BuildingRenovationService renoService)
    : DistroComponentBase(renoService.distroService), IDistroReceiver, IDuplicable<RenovationDistroReceiver>
{
    static readonly ComponentKey ReceiverSaveKey = new(nameof(RenovationDistroReceiver));
    static readonly PropertyKey<int> PriorityKey = new(nameof(Priority));
    static readonly PropertyKey<bool> CollectingKey = new("Collecting");
    static readonly PropertyKey<string> StoredGoodsKey = new("StoredGoods");
    static readonly PropertyKey<string> DemandKey = new("Demand");

    BuildingRenovationComponent controller = null!;

    readonly Dictionary<string, int> stored = [];
    readonly Dictionary<string, int> demand = [];
    bool collecting;

    public Priority Priority { get; private set; } = Priority.Normal;

    public bool IsCollecting => collecting;
    public bool HasDemand => demand.Values.Any(v => v > 0);
    public bool IsFullyStocked => collecting && !HasDemand;

    public IReadOnlyDictionary<string, int> StoredGoods => stored;
    public IReadOnlyDictionary<string, int> RemainingDemand => demand;

    public override IEnumerable<GoodAmount> Goods
    {
        get
        {
            foreach (var (id, amount) in demand)
            {
                if (amount > 0)
                {
                    yield return new(id, amount);
                }
            }
        }
    }

    public override IEnumerable<string> GoodIds => demand.Keys;

    protected override bool CalculateActive()
        => base.CalculateActive()
        && collecting
        && HasDemand;

    public override void Awake()
    {
        base.Awake();
        controller = GetComponent<BuildingRenovationComponent>();
    }

    public void BeginCollecting(IReadOnlyList<GoodAmountSpec> cost, Priority priority)
    {
        stored.Clear();
        demand.Clear();
        collecting = true;

        foreach (var c in cost.Where(q => q.Amount > 0))
        {
            demand[c.Id] = demand.GetValueOrDefault(c.Id) + c.Amount;
        }

        Priority = priority;
        MarkActiveDirty();

        if (IsFullyStocked)
        {
            controller.OnMaterialsFullyStocked();
        }
    }

    /// <summary>Resume collecting after load without wiping already-restored store/demand.</summary>
    public void ResumeCollecting(Priority priority)
    {
        collecting = true;
        Priority = priority;
        MarkActiveDirty();

        if (IsFullyStocked)
        {
            controller.OnMaterialsFullyStocked();
        }
    }

    public void MarkMaterialsCommitted()
    {
        collecting = false;
        demand.Clear();
        MarkActiveDirty();
    }

    public void TransferIn(GoodAmount good)
    {
        if (!collecting || !demand.TryGetValue(good.GoodId, out var remaining) || remaining <= 0)
        {
            return;
        }

        var take = Math.Min(remaining, good.Amount);
        if (take <= 0) { return; }

        stored[good.GoodId] = stored.GetValueOrDefault(good.GoodId) + take;
        remaining -= take;
        if (remaining <= 0)
        {
            demand.Remove(good.GoodId);
        }
        else
        {
            demand[good.GoodId] = remaining;
        }

        MarkActiveDirty();

        if (IsFullyStocked)
        {
            controller.OnMaterialsFullyStocked();
        }
    }

    public string? CanReceiveGood(HashSet<string> goodIds)
    {
        if (!collecting) { return null; }

        foreach (var id in goodIds)
        {
            if (demand.TryGetValue(id, out var remaining) && remaining > 0)
            {
                return id;
            }
        }

        return null;
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
        MarkActiveDirty();
    }

    public void ConsumeStoredGoods()
    {
        stored.Clear();
        demand.Clear();
        collecting = false;
        MarkActiveDirty();
    }

    public void RefundAndClear()
    {
        RefundStoredGoods();
        stored.Clear();
        demand.Clear();
        collecting = false;
        MarkActiveDirty();
    }

    public override void DeleteEntity()
    {
        RefundStoredGoods();
        stored.Clear();
        demand.Clear();
        collecting = false;
        base.DeleteEntity();
    }

    void RefundStoredGoods()
    {
        if (stored.Count == 0) { return; }

        var goods = stored
            .Where(kv => kv.Value > 0)
            .Select(kv => new GoodAmount(kv.Key, kv.Value))
            .ToList();

        if (goods.Count == 0) { return; }

        renoService.goodStackSpawner.AddAwaitingGoods(blockObject.Coordinates, goods);
    }

    public override void Save(IEntitySaver entitySaver)
    {
        base.Save(entitySaver);

        var s = entitySaver.GetComponent(ReceiverSaveKey);
        s.Set(PriorityKey, (int)Priority);
        s.Set(CollectingKey, collecting);
        s.Set(StoredGoodsKey, JsonConvert.SerializeObject(stored));
        s.Set(DemandKey, JsonConvert.SerializeObject(demand));
    }

    public override void Load(IEntityLoader entityLoader)
    {
        base.Load(entityLoader);

        if (!entityLoader.TryGetComponent(ReceiverSaveKey, out var s)) { return; }

        if (s.Has(PriorityKey))
        {
            Priority = (Priority)s.Get(PriorityKey);
        }

        if (s.Has(CollectingKey))
        {
            collecting = s.Get(CollectingKey);
        }

        if (s.Has(StoredGoodsKey))
        {
            LoadDict(s.Get(StoredGoodsKey), stored);
        }

        if (s.Has(DemandKey))
        {
            LoadDict(s.Get(DemandKey), demand);
        }

        MarkActiveDirty();
    }

    static void LoadDict(string json, Dictionary<string, int> target)
    {
        target.Clear();
        var loaded = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        if (loaded is null) { return; }

        foreach (var kv in loaded)
        {
            target[kv.Key] = kv.Value;
        }
    }

    public DistroReceiverSerializableModel Serialize() => new(Enabled, Priority);

    public void Deserialize(DistroReceiverSerializableModel model)
    {
        if (model.Enabled != Enabled)
        {
            SetEnabled(model.Enabled);
        }

        if (model.Priority != Priority)
        {
            SetPriority(model.Priority);
        }
    }

    public void DuplicateFrom(RenovationDistroReceiver source) => Deserialize(source.Serialize());
}
