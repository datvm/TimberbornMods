namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeReceiverSpec))]
public class PostOfficeReceiver(
    PostOfficeService service,
    EventBus events
) : BaseComponent, IPostOfficeJob, IAwakableComponent, IInitializableEntity, IPostLoadableEntity, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PostOfficeReceiver));
    static readonly PropertyKey<bool> AutoKey = new("Auto");
    static readonly PropertyKey<bool> HadHoldKey = new("HadHold");
    static readonly PropertyKey<float> WorkedKey = new("WorkedHours");
    static readonly ListKey<string> ParcelsKey = new("Parcels");
    static readonly ListKey<string> KeepsKey = new("Keeps");

    readonly List<PostOfficeParcel> parcels = [];
    readonly List<PostOfficeAutoRule> keeps = [];

    PostOfficeReceiverSpec spec = null!;
    PostOfficeInventory inventory = null!;
    int? holdId;
    bool listening;
    bool started;
    bool needsResume;
    bool busy;
    bool again;

    public bool AutoEnabled { get; private set; }
    public float WorkedHours { get; private set; }
    public float JobHours => spec.RetrieveHours;
    public int InventoryCapacity => spec.InventoryCapacity;
    public bool HasHold => holdId is not null;
    public IReadOnlyList<PostOfficeParcel> Parcels => parcels;
    public IReadOnlyList<PostOfficeAutoRule> Keeps => keeps;
    public PostOfficeParcel? Active => parcels.Count == 0 ? null : parcels[0];

    public bool IsRunnable
    {
        get
        {
            if (!Ready() || parcels.Count == 0 || holdId is null || WorkedHours >= JobHours)
            {
                return false;
            }

            return !NeedsRoom();
        }
    }

    public bool NeedsRoom()
    {
        if (parcels.Count == 0)
        {
            return false;
        }

        var parcel = service.GoodsWeight(parcels[0].Goods);
        if (parcel > spec.InventoryCapacity)
        {
            return false;
        }

        var stock = inventory.Inventory;
        if (!stock)
        {
            return true;
        }

        return service.StockWeight(stock) + parcel > spec.InventoryCapacity;
    }

    public void Awake()
    {
        spec = GetComponent<PostOfficeReceiverSpec>();
        inventory = GetComponent<PostOfficeInventory>();
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
        ReleaseHold();
        WorkedHours = 0f;
    }

    public void Progress(float hours)
    {
        if (!IsRunnable || hours <= 0f)
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

        if (index == 0)
        {
            ReleaseHold();
            WorkedHours = 0f;
        }

        parcels.RemoveAt(index);
        LookAgain();
    }

    public bool CanCancel(int index)
        => index >= 0 && index < parcels.Count && (index == 0 || !parcels[index].Auto);

    public void SetAuto(bool enabled, IReadOnlyList<PostOfficeAutoRule> next)
    {
        PostOfficeAutoRule[] incoming = [..next];
        AutoEnabled = enabled;
        keeps.Clear();
        foreach (var keep in incoming)
        {
            if (keep.Target <= 0 || keep.Margin < 0 || service.ItemWeight(keep.GoodId) <= 0)
            {
                continue;
            }

            keeps.Add(keep);
        }

        if (keeps.Count == 0)
        {
            AutoEnabled = false;
        }

        RemoveAuto();
        LookAgain();
    }

    public void CopyAccepted(Dictionary<string, int> target)
    {
        if (!AutoEnabled)
        {
            return;
        }

        foreach (var keep in keeps)
        {
            if (keep.Target > 0)
            {
                target[keep.GoodId] = keep.Target;
            }
        }
    }

    public string WaitKey()
    {
        if (parcels.Count == 0)
        {
            return "";
        }

        if (holdId is null)
        {
            return "LV.TPO.WaitingGlobal";
        }

        if (NeedsRoom())
        {
            return "LV.TPO.WaitingSpace";
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

        if (WaitKey() == "LV.TPO.WaitingSpace")
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
        if (holdId is not null || parcels.Count == 0)
        {
            return false;
        }

        foreach (var good in parcels[0].Goods)
        {
            if (service.Available(good.GoodId) <= 0)
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
        saver.Set(WorkedKey, WorkedHours);
        if (parcels.Count > 0)
        {
            saver.Set(ParcelsKey, parcels.Select(PostOfficeSave.WriteParcel).ToList());
        }

        if (keeps.Count > 0)
        {
            saver.Set(KeepsKey, keeps.Select(PostOfficeSave.WriteRule).ToList());
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader))
        {
            return;
        }

        AutoEnabled = loader.Has(AutoKey) && loader.Get(AutoKey);
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

        keeps.Clear();
        if (loader.Has(KeepsKey))
        {
            foreach (var line in loader.Get(KeepsKey))
            {
                if (PostOfficeSave.ReadRule(line) is PostOfficeAutoRule rule)
                {
                    keeps.Add(rule);
                    continue;
                }

                foreach (var good in PostOfficeSave.ReadGoods(line))
                {
                    keeps.Add(new(good.GoodId, good.Amount, 0));
                }
            }
        }

        if (keeps.Count == 0)
        {
            AutoEnabled = false;
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
        if (holdId is not null && WorkedHours >= JobHours && !NeedsRoom())
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
            WorkedHours = 0f;
            return;
        }

        var id = service.TryReserveOutgoing(parcels[0].Goods);
        if (id is null)
        {
            parcels.RemoveAt(0);
            WorkedHours = 0f;
            return;
        }

        holdId = id;
    }

    void RefreshReservation()
    {
        if (parcels.Count == 0)
        {
            ReleaseHold();
            return;
        }

        if (holdId is not null)
        {
            return;
        }

        holdId = service.TryReserveOutgoing(parcels[0].Goods);
    }

    void Finish()
    {
        var parcel = parcels[0];
        var id = holdId!.Value;
        holdId = null;
        WorkedHours = 0f;
        parcels.RemoveAt(0);
        var stock = inventory.Inventory;
        foreach (var good in parcel.Goods)
        {
            stock.GiveExistingIgnoringCapacity(good);
        }

        service.Commit(id);
        inventory.NotifyLimitsChanged();
    }

    void TryQueueAuto()
    {
        if (!AutoEnabled || parcels.Exists(parcel => parcel.Auto) || !BelowMargin())
        {
            return;
        }

        List<GoodAmount> needs = [];
        foreach (var keep in keeps)
        {
            var need = keep.Target - Tracked(keep.GoodId);
            if (need > 0)
            {
                needs.Add(new(keep.GoodId, need));
            }
        }

        if (needs.Count == 0)
        {
            return;
        }

        if (service.PackOne(needs, true) is PostOfficeParcel parcel)
        {
            parcels.Add(parcel);
        }
    }

    void DropUnneededAuto()
    {
        var index = parcels.FindIndex(parcel => parcel.Auto);
        if (index < 0 || (index == 0 && holdId is not null) || AutoStillNeeded(parcels[index]))
        {
            return;
        }

        if (index == 0)
        {
            ReleaseHold();
            WorkedHours = 0f;
        }

        parcels.RemoveAt(index);
    }

    bool AutoStillNeeded(PostOfficeParcel parcel)
    {
        foreach (var good in parcel.Goods)
        {
            var tracked = Tracked(good.GoodId) - good.Amount;
            if (tracked < TargetOf(good.GoodId) - MarginOf(good.GoodId))
            {
                return true;
            }
        }

        return false;
    }

    bool BelowMargin()
    {
        foreach (var keep in keeps)
        {
            if (Tracked(keep.GoodId) < keep.Target - keep.Margin)
            {
                return true;
            }
        }

        return false;
    }

    int Tracked(string id) => StockOf(id) + Outstanding(id);

    int TargetOf(string id)
    {
        foreach (var keep in keeps)
        {
            if (keep.GoodId == id)
            {
                return keep.Target;
            }
        }

        return 0;
    }

    int MarginOf(string id)
    {
        foreach (var keep in keeps)
        {
            if (keep.GoodId == id)
            {
                return keep.Margin;
            }
        }

        return 0;
    }

    void RemoveAuto()
    {
        var index = parcels.FindIndex(parcel => parcel.Auto);
        if (index < 0)
        {
            return;
        }

        if (index == 0)
        {
            ReleaseHold();
            WorkedHours = 0f;
        }

        parcels.RemoveAt(index);
    }

    int StockOf(string id)
    {
        var stock = inventory.Inventory;
        if (!stock)
        {
            return 0;
        }

        return stock.AmountInStock(id);
    }

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

    void ReleaseHold()
    {
        if (holdId is not int id)
        {
            return;
        }

        holdId = null;
        service.Release(id);
    }

    bool Ready()
    {
        Listen();
        var stock = inventory.Inventory;
        return stock && stock.Enabled;
    }

    bool HasState()
        => parcels.Count > 0 || keeps.Count > 0 || AutoEnabled || WorkedHours != 0f || holdId is not null;

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
