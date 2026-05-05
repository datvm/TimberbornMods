namespace ModdableTimberborn.UpdatableEntityStats.Implementations;

public class InventoryPercent : InventoryStat<float>, IPercentStat
{
    public override string Id => "StoragePercent";
    protected override Func<Inventory, float> InventoryFunction => static inv =>
        inv.Capacity == 0 ? 0 : (float)inv.TotalAmountInStock / inv.Capacity;

    public bool TryGetTracker(UpdatableEntityStatComponent? comp, [NotNullWhen(true)] out IEntityPercentStatTracker? tracker)
    {
        if (base.TryGetTracker(comp, out var baseTracker)
            && baseTracker is IEntityPercentStatTracker p)
        {
            tracker = p;
            return true;
        }

        tracker = null;
        return false;
    }

    protected override InventoryStatTracker<float> CreateTracker(Inventory inventory, UpdatableEntityStatComponent comp)
        => new InventoryPercentStatTracker(inventory, comp);
}

public class InventoryPercentStatTracker(Inventory inventory, UpdatableEntityStatComponent comp) 
    : InventoryStatTracker<float>(inventory, GetValue, comp), IEntityPercentStatTracker
{
    static float GetValue(Inventory inv) => inv.Capacity == 0 ? 0 : (float)inv.TotalAmountInStock / inv.Capacity;

    public override string ValueFormatted => Value.ToString("P0");
}