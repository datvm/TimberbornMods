namespace ModdableTimberborn.Helpers;

public static class InventoryFinder
{

    public static InventoryInfo? LookForConstructionSiteInventory(BaseComponent comp)
    {
        var site = comp.GetComponent<ConstructionSite>();
        return site ? new(site, site.Inventory, InventoryType.In) : null;
    }

    public static InventoryInfo? LookForInventories(BaseComponent comp)
    {
        // Do NOT consider ConstructionSite inventory

        if (TryGetInventory<SimpleOutputInventory>(c =>
        {
            var outType = c.GetComponent<SimpleOutputInventorySpec>().IgnorableCapacity
                ? (InventoryType.Out | InventoryType.Unlimited) : InventoryType.Out;

            return new(c, c.Inventory, outType);
        }, out var info)) { return info; }

        if (TryGetInventory<Stockpile>(c => new(c, c.Inventory, InventoryType.Both), out info)) { return info; }
        if (TryGetInventory<Manufactory>(c => new(c, c.Inventory, InventoryType.Both), out info)) { return info; }
        if (TryGetInventory<GoodConsumingBuilding>(c => new(c, c.Inventory, InventoryType.In), out info)) { return info; }
        if (TryGetInventory<WonderInventory>(c => new(c, c.Inventory, InventoryType.In), out info)) { return info; }
        if (TryGetInventory<BreedingPod>(c => new(c, c.Inventory, InventoryType.In), out info)) { return info; }
        if (TryGetInventory<FireworkLauncher>(c => new(c, c.Inventory, InventoryType.In), out info)) { return info; }
        if (TryGetInventory<DistrictCrossingInventory>(c => new(c, c.Inventory, InventoryType.Both | InventoryType.Unlimited), out info)) { return info; }        
        if (TryGetInventory<RecoveredGoodStack>(c => new(c, c.Inventory, InventoryType.Out), out info)) { return info; }
        
        return null;

        bool TryGetInventory<TComp>(Func<TComp, InventoryInfo?> filter, [NotNullWhen(true)] out InventoryInfo? info)
            where TComp : BaseComponent
        {
            var c = comp.GetComponent<TComp>();
            info = null;

            if (!c) { return false; }

            info = filter(c);
            return info is not null;
        }
    }

    public static InventoryInfo? LookForInventory(BaseComponent comp, InventoryType type)
    {
        var inv = LookForInventories(comp);
        return inv.HasValue && inv.Value.Type.HasFlag(type) ? inv : null;
    }

    public static InventoryInfo? LookForInputInventory(BaseComponent comp)
        => LookForInventory(comp, InventoryType.In);

    public static InventoryInfo? LookForOutputInventory(BaseComponent comp)
        => LookForInventory(comp, InventoryType.Out);

}

public readonly record struct InventoryInfo(BaseComponent Component, Inventory Inventory, InventoryType Type);

[Flags]
public enum InventoryType
{
    None = 0,
    In = 1,
    Out = 2,
    Unlimited = 4,
    Both = In | Out,
}