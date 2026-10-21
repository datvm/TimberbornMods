namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeComponent))]
public class PostOfficeQueue(
    PostOfficeGlobalInventoryService globalInventory,
    PostOfficeService offices,
    GoodAmountSerializer goodAmounts,
    EventBus events
) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PostOfficeQueue));
    static readonly PropertyKey<bool> IsSendKey = new("IsSend");
    static readonly PropertyKey<bool> SendAutoKey = new("SendAuto");
    static readonly PropertyKey<bool> RetrieveAutoKey = new("RetrieveAuto");
    static readonly ListKey<string> ParcelsKey = new("Parcels");
    static readonly ListKey<GoodAmount> SendKeepsKey = new("SendKeeps");
    static readonly ListKey<GoodAmount> RetrieveKeepsKey = new("RetrieveKeeps");

    readonly List<PostOfficeParcel> parcels = [];

    PostOfficeSpec spec = null!;
    PostOfficeInventory building = null!;
    PostOfficeActiveParcel active = null!;

    public bool IsSend { get; private set; }
    public bool HasParcels => parcels.Count > 0;
    public bool HasManual => parcels.Any(parcel => !parcel.Auto);
    public PostOfficeParcel? Front => parcels.Count == 0 ? null : parcels[0];
    public IReadOnlyList<PostOfficeParcel> Parcels => parcels;

    public bool SendAutoEnabled { get; private set; }
    public bool RetrieveAutoEnabled { get; private set; }
    public IReadOnlyList<GoodAmount> SendKeeps { get; private set; } = [];
    public IReadOnlyList<GoodAmount> RetrieveKeeps { get; private set; } = [];

    public void Awake()
    {
        spec = GetComponent<PostOfficeSpec>();
        building = GetComponent<PostOfficeInventory>();
        active = GetComponent<PostOfficeActiveParcel>();
    }

    public void InitializeEntity()
    {
        events.Register(this);
        building.Inventory.InventoryStockChanged += OnStock;
        if (spec.Sends)
        {
            RetrieveAutoEnabled = false;
        }
        else
        {
            SendAutoEnabled = false;
        }

        RefreshLimits();
        TryAuto();
    }

    public void DeleteEntity()
    {
        events.Unregister(this);
        if (building.Inventory)
        {
            building.Inventory.InventoryStockChanged -= OnStock;
        }
    }

    public bool TryEnqueue(bool send, IReadOnlyList<GoodAmount> goods)
    {
        if (send != spec.Sends || (HasManual && IsSend != send))
        {
            return false;
        }

        var split = Split(goods);
        if (split.Count == 0)
        {
            return false;
        }

        CancelAutoParcels();
        parcels.AddRange(split);
        IsSend = send;
        RefreshLimits();
        TryAuto();
        active.LookAgain();
        return true;
    }

    public void Cancel(int index)
    {
        if (index < 0 || index >= parcels.Count)
        {
            return;
        }

        if (index == 0)
        {
            active.ReleaseFront(true);
        }

        parcels.RemoveAt(index);
        if (parcels.Count == 0)
        {
            IsSend = false;
        }

        RefreshLimits();
        TryAuto();
        active.LookAgain();
    }

    public void RemoveFront()
    {
        if (parcels.Count == 0)
        {
            return;
        }

        parcels.RemoveAt(0);
        if (parcels.Count == 0)
        {
            IsSend = false;
        }

        RefreshLimits();
        TryAuto();
    }

    public void SetAuto(bool send, bool retrieve, IReadOnlyList<GoodAmount> sendKeeps, IReadOnlyList<GoodAmount> retrieveKeeps)
    {
        if (spec.Sends)
        {
            retrieve = false;
        }
        else
        {
            send = false;
        }

        SendAutoEnabled = send;
        RetrieveAutoEnabled = retrieve;
        SendKeeps = Normalize(sendKeeps);
        RetrieveKeeps = Normalize(retrieveKeeps);
        RefreshLimits();
        TryAuto();
        active.LookAgain();
    }

    public bool FrontIsInStock()
    {
        if (Front is not PostOfficeParcel parcel)
        {
            return false;
        }

        foreach (var good in parcel.Goods)
        {
            if (building.Inventory.AmountInStock(good.GoodId) < good.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool FrontIsUnreserved()
    {
        if (Front is not PostOfficeParcel parcel)
        {
            return false;
        }

        foreach (var good in parcel.Goods)
        {
            if (building.Inventory.UnreservedAmountInStock(good.GoodId) < good.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool FrontFits()
    {
        if (Front is not PostOfficeParcel parcel)
        {
            return false;
        }

        return globalInventory.GetItemWeight(parcel.Goods) <= FreeWeight();
    }

    public bool AutoSendSurplus()
    {
        if (!IsSend || Front is not PostOfficeParcel parcel || !parcel.Auto)
        {
            return false;
        }

        return !ParcelNeeded(parcel);
    }

    public bool ParcelNeeded(PostOfficeParcel parcel)
    {
        if (!parcel.Auto || !IsSend)
        {
            return true;
        }

        foreach (var good in parcel.Goods)
        {
            var keep = KeepOf(good.GoodId);
            if (keep is null || globalInventory.Inventory[good.GoodId].Amount < keep.Value)
            {
                return true;
            }
        }

        return false;
    }

    public bool GlobalHasFront()
    {
        if (Front is not PostOfficeParcel parcel)
        {
            return false;
        }

        foreach (var good in parcel.Goods)
        {
            if (globalInventory.Inventory[good.GoodId].Amount < good.Amount)
            {
                return false;
            }
        }

        return true;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasState())
        {
            return;
        }

        var saver = entitySaver.GetComponent(SaveKey);
        saver.Set(IsSendKey, IsSend);
        saver.Set(SendAutoKey, SendAutoEnabled);
        saver.Set(RetrieveAutoKey, RetrieveAutoEnabled);
        if (parcels.Count > 0)
        {
            saver.Set(ParcelsKey, parcels.Select(WriteParcel).ToList());
        }

        if (SendKeeps.Count > 0)
        {
            saver.Set(SendKeepsKey, SendKeeps, goodAmounts);
        }

        if (RetrieveKeeps.Count > 0)
        {
            saver.Set(RetrieveKeepsKey, RetrieveKeeps, goodAmounts);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader))
        {
            return;
        }

        IsSend = loader.Has(IsSendKey) && loader.Get(IsSendKey);
        SendAutoEnabled = loader.Has(SendAutoKey) && loader.Get(SendAutoKey);
        RetrieveAutoEnabled = loader.Has(RetrieveAutoKey) && loader.Get(RetrieveAutoKey);
        if (SendAutoEnabled)
        {
            RetrieveAutoEnabled = false;
        }

        parcels.Clear();
        if (loader.Has(ParcelsKey))
        {
            foreach (var line in loader.Get(ParcelsKey))
            {
                if (ReadParcel(line) is PostOfficeParcel parcel)
                {
                    parcels.Add(parcel);
                }
            }
        }

        if (parcels.Count == 0)
        {
            IsSend = false;
        }

        SendKeeps = loader.Has(SendKeepsKey) ? loader.Get(SendKeepsKey, goodAmounts) : [];
        RetrieveKeeps = loader.Has(RetrieveKeepsKey) ? loader.Get(RetrieveKeepsKey, goodAmounts) : [];
    }

    [OnEvent]
    public void OnGlobalInventoryChanged(OnPostOfficeGlobalInventoryChanged _)
    {
        if (active.IsBusy)
        {
            return;
        }

        RefreshLimits();
        TryAuto();
        active.LookAgain();
    }

    [OnEvent]
    public void OnCapacityChanged(OnPostOfficeCapacityChanged _)
    {
        if (active.IsBusy)
        {
            return;
        }

        active.LookAgain();
    }

    void OnStock(object sender, InventoryStockChangedEventArgs _)
    {
        if (active.IsBusy)
        {
            return;
        }

        RefreshLimits();
        TryAuto();
        active.LookAgain();
    }

    void RefreshLimits()
    {
        if (!building.Inventory)
        {
            return;
        }

        building.SetAllowed(IsSend ? SendAllowed() : RetrieveAllowed());
    }

    List<GoodAmount> SendAllowed()
    {
        var stockLeft = new Dictionary<string, int>();
        foreach (var good in building.Inventory.Stock)
        {
            stockLeft[good.GoodId] = stockLeft.GetValueOrDefault(good.GoodId) + good.Amount;
        }

        var wanted = new Dictionary<string, int>();
        var budget = FreeWeight();
        foreach (var parcel in parcels)
        {
            foreach (var good in parcel.Goods)
            {
                var have = stockLeft.GetValueOrDefault(good.GoodId);
                var use = Math.Min(good.Amount, have);
                if (use > 0)
                {
                    stockLeft[good.GoodId] = have - use;
                    wanted[good.GoodId] = wanted.GetValueOrDefault(good.GoodId) + use;
                }

                var need = good.Amount - use;
                var unit = globalInventory.GetItemWeight(good.GoodId);
                if (!ParcelNeeded(parcel) || need <= 0 || unit <= 0 || budget <= 0)
                {
                    continue;
                }

                var extra = Math.Min(need, budget / unit);
                if (extra <= 0)
                {
                    continue;
                }

                budget -= extra * unit;
                wanted[good.GoodId] = wanted.GetValueOrDefault(good.GoodId) + extra;
            }
        }

        return wanted.Select(pair => new GoodAmount(pair.Key, pair.Value)).ToList();
    }

    List<GoodAmount> RetrieveAllowed()
    {
        if (!RetrieveAutoEnabled || RetrieveKeeps.Count == 0)
        {
            return [];
        }

        List<GoodAmount> allowed = [];
        var budget = spec.InventoryCapacity;
        foreach (var keep in RetrieveKeeps)
        {
            var unit = globalInventory.GetItemWeight(keep.GoodId);
            if (keep.Amount <= 0 || unit <= 0 || budget <= 0)
            {
                continue;
            }

            var amount = Math.Min(keep.Amount, budget / unit);
            if (amount <= 0)
            {
                continue;
            }

            budget -= amount * unit;
            allowed.Add(new(keep.GoodId, amount));
        }

        return allowed;
    }

    void CancelAutoParcels()
    {
        if (parcels.Count > 0 && parcels[0].Auto)
        {
            active.ReleaseFront(true);
        }

        parcels.RemoveAll(parcel => parcel.Auto);
        if (parcels.Count == 0)
        {
            IsSend = false;
        }
    }

    void TryAuto()
    {
        if (HasParcels)
        {
            return;
        }

        if (spec.Sends)
        {
            if (!SendAutoEnabled)
            {
                return;
            }

            var parcel = PackAuto(SendKeeps, true);
            if (parcel is null)
            {
                return;
            }

            parcels.Add(parcel.Value);
            IsSend = true;
            RefreshLimits();
            return;
        }

        if (!RetrieveAutoEnabled)
        {
            return;
        }

        var retrieve = PackAuto(RetrieveKeeps, false);
        if (retrieve is null)
        {
            return;
        }

        parcels.Add(retrieve.Value);
        IsSend = false;
        RefreshLimits();
    }

    PostOfficeParcel? PackAuto(IReadOnlyList<GoodAmount> keeps, bool send)
    {
        List<GoodAmount> goods = [];
        var budget = offices.ParcelWeight;
        if (!send)
        {
            budget = Math.Min(budget, Math.Max(0, spec.InventoryCapacity - StockWeight() - QueuedWeight()));
        }

        foreach (var keep in keeps)
        {
            if (budget <= 0)
            {
                break;
            }

            var unit = globalInventory.GetItemWeight(keep.GoodId);
            if (keep.Amount <= 0 || unit <= 0)
            {
                continue;
            }

            var have = send
                ? globalInventory.Inventory[keep.GoodId].Amount
                : building.Inventory.AmountInStock(keep.GoodId);
            var need = keep.Amount - have - QueuedAmount(keep.GoodId);
            if (need <= 0)
            {
                continue;
            }

            var fit = Math.Min(need, budget / unit);
            if (fit <= 0)
            {
                continue;
            }

            goods.Add(new(keep.GoodId, fit));
            budget -= fit * unit;
        }

        if (goods.Count == 0)
        {
            return null;
        }

        return new PostOfficeParcel(goods, true);
    }

    List<PostOfficeParcel> Split(IReadOnlyList<GoodAmount> goods)
    {
        var left = new List<GoodAmount>();
        foreach (var good in goods)
        {
            var unit = globalInventory.GetItemWeight(good.GoodId);
            if (good.Amount <= 0 || unit <= 0 || unit > offices.ParcelWeight)
            {
                continue;
            }

            left.Add(good);
        }

        List<PostOfficeParcel> result = [];
        while (left.Count > 0)
        {
            List<GoodAmount> parcel = [];
            var budget = offices.ParcelWeight;
            for (var i = 0; i < left.Count && budget > 0; i++)
            {
                var good = left[i];
                var unit = globalInventory.GetItemWeight(good.GoodId);
                var fit = Math.Min(good.Amount, budget / unit);
                if (fit <= 0)
                {
                    continue;
                }

                parcel.Add(new(good.GoodId, fit));
                budget -= fit * unit;
                left[i] = new(good.GoodId, good.Amount - fit);
            }

            left.RemoveAll(good => good.Amount <= 0);
            if (parcel.Count == 0)
            {
                break;
            }

            result.Add(new(parcel, false));
        }

        return result;
    }

    int FreeWeight() => Math.Max(0, spec.InventoryCapacity - StockWeight());

    int StockWeight()
    {
        var sum = 0;
        foreach (var good in building.Inventory.Stock)
        {
            sum += globalInventory.GetItemWeight(good.GoodId, good.Amount);
        }

        return sum;
    }

    int QueuedWeight()
    {
        var sum = 0;
        foreach (var parcel in parcels)
        {
            sum += globalInventory.GetItemWeight(parcel.Goods);
        }

        return sum;
    }

    int? KeepOf(string id)
    {
        foreach (var keep in SendKeeps)
        {
            if (keep.GoodId == id)
            {
                return keep.Amount;
            }
        }

        return null;
    }

    int QueuedAmount(string id)
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

    static List<GoodAmount> Normalize(IReadOnlyList<GoodAmount> goods)
    {
        List<GoodAmount> result = [];
        foreach (var good in goods)
        {
            if (good.Amount > 0)
            {
                result.Add(good);
            }
        }

        return result;
    }

    bool HasState()
        => HasParcels
            || SendAutoEnabled
            || RetrieveAutoEnabled
            || SendKeeps.Count > 0
            || RetrieveKeeps.Count > 0;

    static string WriteParcel(PostOfficeParcel parcel)
    {
        var parts = new List<string> { parcel.Auto ? "1" : "0" };
        foreach (var good in parcel.Goods)
        {
            parts.Add(good.GoodId);
            parts.Add(good.Amount.ToString());
        }

        return string.Join("|", parts);
    }

    static PostOfficeParcel? ReadParcel(string line)
    {
        var parts = line.Split('|');
        if (parts.Length < 3 || (parts.Length - 1) % 2 != 0)
        {
            return null;
        }

        List<GoodAmount> goods = [];
        for (var i = 1; i < parts.Length; i += 2)
        {
            if (!int.TryParse(parts[i + 1], out var amount) || amount <= 0 || parts[i].Length == 0)
            {
                continue;
            }

            goods.Add(new(parts[i], amount));
        }

        if (goods.Count == 0)
        {
            return null;
        }

        return new PostOfficeParcel(goods, parts[0] == "1");
    }
}
