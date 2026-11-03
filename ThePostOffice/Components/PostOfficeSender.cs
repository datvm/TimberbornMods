namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeSenderSpec))]
public class PostOfficeSender(
    PostOfficeService service,
    EventBus events
) : BaseComponent, IPostOfficeJob, IAwakableComponent, IInitializableEntity, IPostLoadableEntity, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PostOfficeSender));
    static readonly PropertyKey<bool> AutoKey = new("Auto");
    static readonly PropertyKey<bool> HadHoldKey = new("HadHold");
    static readonly PropertyKey<bool> GoodsTakenKey = new("GoodsTaken");
    static readonly PropertyKey<float> WorkedKey = new("WorkedHours");
    static readonly ListKey<string> ParcelsKey = new("Parcels");
    static readonly ListKey<string> RulesKey = new("Rules");
    static readonly ListKey<string> TakenKey = new("Taken");

    readonly List<PostOfficeParcel> parcels = [];
    readonly List<PostOfficeAutoRule> rules = [];
    readonly List<GoodAmount> taken = [];

    PostOfficeSenderSpec spec = null!;
    PostOfficeInventory inventory = null!;
    BlockObject block = null!;
    DistrictBuilding district = null!;
    int? holdId;
    bool listening;
    bool started;
    bool needsResume;
    bool busy;
    bool again;

    public bool AutoEnabled { get; private set; }
    public bool GoodsTaken { get; private set; }
    public float WorkedHours { get; private set; }
    public float JobHours => spec.SendHours;
    public bool HasHold => holdId is not null;
    public IReadOnlyList<PostOfficeParcel> Parcels => parcels;
    public IReadOnlyList<PostOfficeAutoRule> Rules => rules;
    public PostOfficeParcel? Active => parcels.Count == 0 ? null : parcels[0];

    public bool IsRunnable
    {
        get
        {
            if (!Ready() || parcels.Count == 0 || holdId is null || WorkedHours >= JobHours)
            {
                return false;
            }

            if (GoodsTaken)
            {
                return true;
            }

            return Complete(parcels[0]);
        }
    }

    public void Awake()
    {
        spec = GetComponent<PostOfficeSenderSpec>();
        inventory = GetComponent<PostOfficeInventory>();
        block = GetComponent<BlockObject>();
        district = GetComponent<DistrictBuilding>();
    }

    public void InitializeEntity()
    {
        events.Register(this);
        Listen();
    }

    public void PostLoadEntity()
    {
        started = true;
        LookAgain();
    }

    public void DeleteEntity()
    {
        events.Unregister(this);
        Unlisten();
        if (GoodsTaken)
        {
            SpawnTaken();
        }

        ReleaseHold();
    }

    public void NotifyFinished()
    {
        if (!started)
        {
            return;
        }

        LookAgain();
    }

    public void OnBuildingUnfinished()
    {
        if (!GoodsTaken && holdId is null)
        {
            return;
        }

        if (GoodsTaken)
        {
            SpawnTaken();
        }

        ReleaseHold();
        WorkedHours = 0f;
        if (parcels.Count > 0)
        {
            parcels.RemoveAt(0);
        }
    }

    public void Progress(float hours)
    {
        if (!IsRunnable || hours <= 0f)
        {
            return;
        }

        if (!GoodsTaken && !TakeGoods())
        {
            return;
        }

        WorkedHours += hours;
        if (WorkedHours >= JobHours)
        {
            Finish();
        }
    }

    public bool Enqueue(IReadOnlyList<GoodAmount> goods)
    {
        var split = service.Split(goods);
        if (split.Count == 0)
        {
            return false;
        }

        InsertManuals(split);
        LookAgain();
        return true;
    }

    public void Cancel(int index)
    {
        if (!CanCancel(index))
        {
            return;
        }

        DropAt(index);
        parcels.RemoveAt(index);
        LookAgain();
    }

    public bool CanCancel(int index)
        => index >= 0 && index < parcels.Count && (index == 0 || !parcels[index].Auto);

    public void SetAuto(bool enabled, IReadOnlyList<PostOfficeAutoRule> next)
    {
        PostOfficeAutoRule[] incoming = [..next];
        AutoEnabled = enabled;
        rules.Clear();
        foreach (var rule in incoming)
        {
            if (rule.Target <= 0 || rule.Margin < 0 || service.ItemWeight(rule.GoodId) <= 0)
            {
                continue;
            }

            rules.Add(rule);
        }

        if (rules.Count == 0)
        {
            AutoEnabled = false;
        }

        RemoveQueuedAuto();
        LookAgain();
    }

    public void CopyAccepted(Dictionary<string, int> target)
    {
        var index = GatheringIndex();
        if (index >= 0)
        {
            foreach (var good in parcels[index].Goods)
            {
                target[good.GoodId] = target.GetValueOrDefault(good.GoodId) + good.Amount;
            }
        }

        var stock = inventory.Inventory;
        if (!stock)
        {
            return;
        }

        foreach (var good in stock.Stock)
        {
            if (good.Amount > target.GetValueOrDefault(good.GoodId))
            {
                target[good.GoodId] = good.Amount;
            }
        }
    }

    public string WaitKey()
    {
        if (parcels.Count == 0)
        {
            return "";
        }

        if (!GoodsTaken && !Complete(parcels[0]))
        {
            return "LV.TPO.WaitingStock";
        }

        if (holdId is null)
        {
            return "LV.TPO.WaitingWeight";
        }

        return "LV.TPO.Working";
    }

    public PostOfficeStall Stall()
    {
        if (!Ready())
        {
            return PostOfficeStall.None;
        }

        if (parcels.Count == 0)
        {
            return AutoEnabled ? PostOfficeStall.None : PostOfficeStall.NothingToDo;
        }

        if (WaitKey() == "LV.TPO.WaitingWeight")
        {
            return PostOfficeStall.NoStorage;
        }

        if (GoodsAreMissing())
        {
            return PostOfficeStall.LackOfResources;
        }

        return PostOfficeStall.None;
    }

    bool GoodsAreMissing()
    {
        if (GoodsTaken || parcels.Count == 0 || Complete(parcels[0]) || !district || !district.District)
        {
            return false;
        }

        var registry = district.District.GetComponent<DistrictInventoryRegistry>();
        var stock = inventory.Inventory;
        foreach (var good in parcels[0].Goods)
        {
            if (stock.UnreservedAmountInStock(good.GoodId) >= good.Amount)
            {
                continue;
            }

            if (registry.ActiveInventoriesWithStock(good.GoodId).Count == 0)
            {
                return true;
            }
        }

        return false;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasState())
        {
            return;
        }

        var saver = entitySaver.GetComponent(SaveKey);
        saver.Set(AutoKey, AutoEnabled);
        saver.Set(HadHoldKey, holdId is not null);
        saver.Set(GoodsTakenKey, GoodsTaken);
        saver.Set(WorkedKey, WorkedHours);
        if (parcels.Count > 0)
        {
            saver.Set(ParcelsKey, parcels.Select(PostOfficeSave.WriteParcel).ToList());
        }

        if (rules.Count > 0)
        {
            saver.Set(RulesKey, rules.Select(PostOfficeSave.WriteRule).ToList());
        }

        if (taken.Count > 0)
        {
            saver.Set(TakenKey, [PostOfficeSave.WriteGoods(taken)]);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader))
        {
            return;
        }

        AutoEnabled = loader.Has(AutoKey) && loader.Get(AutoKey);
        GoodsTaken = loader.Has(GoodsTakenKey) && loader.Get(GoodsTakenKey);
        WorkedHours = loader.Has(WorkedKey) ? loader.Get(WorkedKey) : 0f;
        needsResume = loader.Has(HadHoldKey) && loader.Get(HadHoldKey);
        parcels.Clear();
        if (loader.Has(ParcelsKey))
        {
            foreach (var line in loader.Get(ParcelsKey))
            {
                if (PostOfficeSave.ReadParcel(line) is PostOfficeParcel parcel)
                {
                    parcels.Add(parcel);
                }
            }
        }

        rules.Clear();
        if (loader.Has(RulesKey))
        {
            foreach (var line in loader.Get(RulesKey))
            {
                if (PostOfficeSave.ReadRule(line) is PostOfficeAutoRule rule)
                {
                    rules.Add(rule);
                }
            }
        }

        if (rules.Count == 0)
        {
            AutoEnabled = false;
        }

        taken.Clear();
        if (loader.Has(TakenKey))
        {
            foreach (var line in loader.Get(TakenKey))
            {
                taken.AddRange(PostOfficeSave.ReadGoods(line));
            }
        }

        if (taken.Count == 0)
        {
            GoodsTaken = false;
        }
    }

    [OnEvent]
    public void OnGlobalInventoryChanged(OnPostOfficeGlobalInventoryChanged _)
    {
        if (!started)
        {
            return;
        }

        LookAgain();
    }

    [OnEvent]
    public void OnCapacityChanged(OnPostOfficeCapacityChanged _)
    {
        if (!started)
        {
            return;
        }

        LookAgain();
    }

    [OnEvent]
    public void OnHoldsReleased(OnPostOfficeHoldsReleased released)
    {
        if (!started || holdId is not int id || !Contains(released.Ids, id))
        {
            return;
        }

        holdId = null;
        if (GoodsTaken)
        {
            SpawnTaken();
        }
        else if (parcels.Count > 0)
        {
            DropFromBuilding(parcels[0].Goods);
        }

        WorkedHours = 0f;
        if (parcels.Count > 0)
        {
            parcels.RemoveAt(0);
        }

        LookAgain();
    }

    void LookAgain()
    {
        if (!started || !Ready())
        {
            return;
        }

        if (busy)
        {
            again = true;
            return;
        }

        busy = true;
        try
        {
            do
            {
                again = false;
                if (needsResume)
                {
                    needsResume = false;
                    ResumeHold();
                }

                Step();
            }
            while (again);
        }
        finally
        {
            busy = false;
        }
    }

    void Step()
    {
        if (GoodsTaken && holdId is not null && WorkedHours >= JobHours)
        {
            Finish();
            return;
        }

        DropUnneededAuto();
        TryQueueAuto();
        RefreshReservation();
        inventory.NotifyLimitsChanged();
    }

    void ResumeHold()
    {
        if (parcels.Count == 0)
        {
            ClearJob();
            return;
        }

        IReadOnlyList<GoodAmount> goods = taken;
        if (!GoodsTaken)
        {
            goods = parcels[0].Goods;
        }
        if (!GoodsTaken && !Complete(parcels[0]))
        {
            DropFromBuilding(parcels[0].Goods);
            parcels.RemoveAt(0);
            ClearJob();
            return;
        }

        var id = service.TryReserveIncoming(goods);
        if (id is null)
        {
            if (GoodsTaken)
            {
                SpawnTaken();
            }
            else if (parcels.Count > 0)
            {
                DropFromBuilding(parcels[0].Goods);
            }

            if (parcels.Count > 0)
            {
                parcels.RemoveAt(0);
            }

            ClearJob();
            return;
        }

        holdId = id;
    }

    void RefreshReservation()
    {
        if (GoodsTaken)
        {
            if (holdId is null && taken.Count > 0)
            {
                holdId = service.TryReserveIncoming(taken);
            }

            return;
        }

        if (parcels.Count == 0 || !Complete(parcels[0]))
        {
            ReleaseHold();
            return;
        }

        if (holdId is null)
        {
            holdId = service.TryReserveIncoming(parcels[0].Goods);
        }
    }

    void Finish()
    {
        var id = holdId!.Value;
        holdId = null;
        GoodsTaken = false;
        taken.Clear();
        WorkedHours = 0f;
        if (parcels.Count > 0)
        {
            parcels.RemoveAt(0);
        }

        service.Commit(id);
        inventory.NotifyLimitsChanged();
    }

    bool TakeGoods()
    {
        var stock = inventory.Inventory;
        var parcel = parcels[0];
        foreach (var good in parcel.Goods)
        {
            if (stock.UnreservedAmountInStock(good.GoodId) < good.Amount)
            {
                return false;
            }
        }

        taken.Clear();
        taken.AddRange(parcel.Goods);
        GoodsTaken = true;
        foreach (var good in parcel.Goods)
        {
            stock.TakeExisting(good);
        }

        inventory.NotifyLimitsChanged();
        return true;
    }

    void TryQueueAuto()
    {
        if (!AutoEnabled || parcels.Exists(parcel => parcel.Auto) || !BelowMargin())
        {
            return;
        }

        List<GoodAmount> needs = [];
        foreach (var rule in rules)
        {
            var need = rule.Target - Tracked(rule.GoodId);
            if (need > 0)
            {
                needs.Add(new(rule.GoodId, need));
            }
        }

        if (service.PackOne(needs, true) is PostOfficeParcel parcel)
        {
            parcels.Add(parcel);
        }
    }

    void DropUnneededAuto()
    {
        var index = parcels.FindIndex(parcel => parcel.Auto);
        if (index < 0 || (index == 0 && GoodsTaken) || AutoStillNeeded(parcels[index]))
        {
            return;
        }

        DropAt(index);
        parcels.RemoveAt(index);
    }

    void RemoveQueuedAuto()
    {
        var index = parcels.FindIndex(parcel => parcel.Auto);
        if (index < 0 || (index == 0 && GoodsTaken))
        {
            return;
        }

        DropAt(index);
        parcels.RemoveAt(index);
    }

    bool AutoStillNeeded(PostOfficeParcel parcel)
    {
        foreach (var good in parcel.Goods)
        {
            var tracked = service.GlobalAmount(good.GoodId) + Outstanding(good.GoodId) - good.Amount;
            if (tracked < TargetOf(good.GoodId))
            {
                return true;
            }
        }

        return false;
    }

    bool BelowMargin()
    {
        foreach (var rule in rules)
        {
            if (Tracked(rule.GoodId) < rule.Target - rule.Margin)
            {
                return true;
            }
        }

        return false;
    }

    int Tracked(string id) => service.GlobalAmount(id) + Outstanding(id);

    int Outstanding(string id)
    {
        var sum = 0;
        foreach (var parcel in parcels)
        {
            foreach (var good in parcel.Goods)
            {
                if (good.GoodId == id)
                {
                    sum += good.Amount;
                }
            }
        }

        return sum;
    }

    int TargetOf(string id)
    {
        foreach (var rule in rules)
        {
            if (rule.GoodId == id)
            {
                return rule.Target;
            }
        }

        return 0;
    }

    void InsertManuals(List<PostOfficeParcel> split)
    {
        var autoAt = parcels.FindIndex(parcel => parcel.Auto);
        var at = autoAt < 0 ? parcels.Count : autoAt;
        if (at == 0 && parcels.Count > 0)
        {
            at = 1;
        }

        parcels.InsertRange(at, split);
    }

    void DropAt(int index)
    {
        if (index == 0 && GoodsTaken)
        {
            SpawnTaken();
            ReleaseHold();
            WorkedHours = 0f;
            return;
        }

        if (index == 0)
        {
            ReleaseHold();
            WorkedHours = 0f;
        }

        var gathering = GatheringIndex();
        if (index == 0 || index == gathering)
        {
            DropFromBuilding(parcels[index].Goods);
        }
    }

    void DropFromBuilding(IReadOnlyList<GoodAmount> goods)
    {
        var stock = inventory.Inventory;
        if (!stock)
        {
            return;
        }

        List<GoodAmount> dropped = [];
        foreach (var good in goods)
        {
            var take = Math.Min(stock.UnreservedAmountInStock(good.GoodId), good.Amount);
            if (take <= 0)
            {
                continue;
            }

            var amount = new GoodAmount(good.GoodId, take);
            stock.TakeExisting(amount);
            dropped.Add(amount);
        }

        service.Spawn(block.Coordinates, dropped);
    }

    void SpawnTaken()
    {
        if (taken.Count == 0)
        {
            GoodsTaken = false;
            return;
        }

        service.Spawn(block.Coordinates, taken);
        taken.Clear();
        GoodsTaken = false;
    }

    void ReleaseHold()
    {
        if (holdId is not int id)
        {
            return;
        }

        holdId = null;
        service.Release(id);
    }

    void ClearJob()
    {
        GoodsTaken = false;
        taken.Clear();
        WorkedHours = 0f;
        holdId = null;
    }

    int GatheringIndex()
    {
        if (parcels.Count == 0)
        {
            return -1;
        }

        if (!GoodsTaken)
        {
            return 0;
        }

        return parcels.Count >= 2 ? 1 : -1;
    }

    bool Complete(PostOfficeParcel parcel)
    {
        var stock = inventory.Inventory;
        if (!stock)
        {
            return false;
        }

        foreach (var good in parcel.Goods)
        {
            if (stock.UnreservedAmountInStock(good.GoodId) < good.Amount)
            {
                return false;
            }
        }

        return true;
    }

    bool Ready()
    {
        Listen();
        var stock = inventory.Inventory;
        return stock && stock.Enabled;
    }

    bool HasState()
        => parcels.Count > 0
            || rules.Count > 0
            || AutoEnabled
            || GoodsTaken
            || WorkedHours != 0f
            || holdId is not null;

    void Listen()
    {
        if (listening)
        {
            return;
        }

        var stock = inventory.Inventory;
        if (!stock)
        {
            return;
        }

        stock.InventoryStockChanged += OnStock;
        listening = true;
    }

    void Unlisten()
    {
        if (!listening)
        {
            return;
        }

        var stock = inventory.Inventory;
        if (stock)
        {
            stock.InventoryStockChanged -= OnStock;
        }

        listening = false;
    }

    void OnStock(object sender, InventoryStockChangedEventArgs _)
    {
        if (!started)
        {
            return;
        }

        LookAgain();
    }

    static bool Contains(ImmutableArray<int> ids, int id)
    {
        foreach (var candidate in ids)
        {
            if (candidate == id)
            {
                return true;
            }
        }

        return false;
    }
}
