namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public abstract class InventoryStat<T> : UpdatableEntityStatBase<T>
{
    protected abstract Func<Inventory, T> InventoryFunction { get; }

    public override bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityStatTracker<T>? tracker)
    {
        tracker = null;
        if (!comp) { return false; }

        var inventory = InventoryFinder.LookForInventories(comp!);
        if (inventory is null) { return false; }

        tracker = CreateTracker(inventory.Value.Inventory, comp!);
        return true;
    }

    protected virtual InventoryStatTracker<T> CreateTracker(Inventory inventory, UpdatableEntityStatComponent comp)
        => new(inventory, InventoryFunction, comp);

    public override bool CanTrack(UpdatableEntityStatComponent? comp)
        => comp && InventoryFinder.LookForInventories(comp!) is not null;

}

public class InventoryStockStat : InventoryStat<int>
{
    public override string Id => "StorageStock";
    protected override Func<Inventory, int> InventoryFunction => static inv => inv.TotalAmountInStock;
}

public class InventoryStockMax : InventoryStat<int>
{
    public override string Id => "StorageStockMax";
    protected override Func<Inventory, int> InventoryFunction => static inv => inv.Capacity;
}

public class InventoryStatTracker<T>(Inventory inventory, Func<Inventory, T> func, UpdatableEntityStatComponent comp) : StatTrackerBase<T>(comp)
{

    protected override void OnStart()
    {
        inventory.InventoryChanged += OnInventoryChanged;
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) => UpdateValue();

    protected override T CalculateValue() => func(inventory);

    protected override void OnPause()
    {
        inventory.InventoryChanged -= OnInventoryChanged;
    }

}
