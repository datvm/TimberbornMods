namespace StockpileBalancer.Components;

[AddTemplateModule2(typeof(Stockpile))]
public class StockpileBalancerComponent(StockpileBalancerService service)
    : BaseComponent, IInitializableEntity, IFinishedStateListener, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(StockpileBalancerComponent));
    static readonly PropertyKey<bool> DisabledKey = new("Disabled");

#nullable disable
    Stockpile stockpile;
    SingleGoodAllower singleGoodAllower;
    Inventory inventory;
#nullable enable

    /// <summary>Axis-aligned occupied volume (stockpiles are rectangular).</summary>
    public BoundsInt Bounds { get; private set; }

    public string? GoodId { get; private set; }
    public int CurrentAmount => GoodId is null ? 0 : inventory.UnreservedAmountInStock(GoodId);
    public int FreeCapacity => GoodId is null ? 0 : inventory.UnreservedCapacity(GoodId);

    public BalancerGroup? BalancerGroup { get; internal set; }

    public bool BalancerDisabled { get; private set; }
    public DistroBalancerConnectionMode ConnectionMode { get; private set; } = DistroBalancerConnectionMode.None;

    public bool HasBalancer => ConnectionMode != DistroBalancerConnectionMode.None;

    /// <summary>
    /// Eligible to sit in the balancer graph: renovated, a good selected,
    /// and no residual (unwanted) stock from a previous good type.
    /// Independent of <see cref="BalancerDisabled"/> — disabled still bridges connections.
    /// </summary>
    public bool IsClusterable => HasBalancer && GoodId is not null && !inventory.HasUnwantedStock;

    /// <summary>Last membership flag pushed to the service (avoids rewiring on every transfer tick).</summary>
    bool lastClusterable;

    public void InitializeEntity()
    {
        Bounds = GetComponent<BlockObject>().GetBounds();

        stockpile = GetComponent<Stockpile>();
        singleGoodAllower = GetComponent<SingleGoodAllower>();
        var inv = inventory = stockpile.Inventory;
        GoodId = GetInventoryGoodId();

        inv.InventoryCapacityReservationChanged += (_, _) => OnInventoryAmountChanged();
        inv.InventoryStockChanged += (_, _) => OnInventoryAmountChanged();

        inv.InventoryDisabled += (_, _) => OnInventoryGoodTypeChanged();
        inv.InventoryEnabled += (_, _) => OnInventoryGoodTypeChanged();
        inv.UnwantedStockDisappeared += (_, _) => OnUnwantedStockDisappeared();
        inv._goodDisallower.DisallowedGoodsChanged += (_, _) => OnInventoryGoodTypeChanged();

        DisableComponent();
    }

    public void SetConnectionMode(DistroBalancerConnectionMode mode)
    {
        if (mode == ConnectionMode) { return; }

        ConnectionMode = mode;
        UpdateConnectionState();
    }

    public void SetBalancerDisabled(bool disabled)
    {
        if (disabled == BalancerDisabled) { return; }

        BalancerDisabled = disabled;
        // Topology unchanged; only transfer participation flips.
        BalancerGroup?.MarkDirty();
    }

    string? GetInventoryGoodId()
        => singleGoodAllower.HasAllowedGood ? singleGoodAllower.AllowedGood : null;

    void OnInventoryAmountChanged()
    {
        // Residual Logs→Planks: HasUnwantedStock may clear on stock changes without GoodId changing.
        if (IsClusterable != lastClusterable)
        {
            UpdateConnectionState();
            return;
        }

        BalancerGroup?.MarkDirty();
    }

    void OnUnwantedStockDisappeared()
    {
        // Inventory already flipped HasUnwantedStock before this event (and before StockChanged).
        if (IsClusterable != lastClusterable)
        {
            UpdateConnectionState();
        }
    }

    void OnInventoryGoodTypeChanged()
    {
        GoodId = GetInventoryGoodId();
        // GoodId and/or HasUnwantedStock may both change on allow/disallow.
        UpdateConnectionState();
    }

    void UpdateConnectionState()
    {
        if (ConnectionMode == DistroBalancerConnectionMode.None)
        {
            DisableComponent();
        }
        else
        {
            EnableComponent();
        }

        service.OnBalancerUpdated(this);
        lastClusterable = IsClusterable;
    }

    public void AddGood(int amount)
    {
        if (GoodId is null || amount <= 0) { return; }
        inventory.GiveExisting(new(GoodId, amount));
    }

    public void RemoveGood(int amount)
    {
        if (GoodId is null || amount <= 0) { return; }
        inventory.TakeExisting(new(GoodId, amount));
    }

    public void OnEnterFinishedState() { } // Upgrades cannot pre-exist on unfinished buildings.

    public void OnExitFinishedState()
    {
        SetConnectionMode(DistroBalancerConnectionMode.None);
    }

    public void DeleteEntity()
    {
        ConnectionMode = DistroBalancerConnectionMode.None;
        service.OnBalancerUpdated(this);
        lastClusterable = false;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (ConnectionMode == DistroBalancerConnectionMode.None) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DisabledKey, BalancerDisabled);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(DisabledKey))
        {
            BalancerDisabled = s.Get(DisabledKey);
        }
    }
}
