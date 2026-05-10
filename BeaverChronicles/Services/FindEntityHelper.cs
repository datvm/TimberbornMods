namespace BeaverChronicles.Services;

[BindSingleton]
public class FindEntityHelper(
    DistrictCenterRegistry districtCenterRegistry,
    BeaverPopulation beaverPopulation,
    DefaultEntityTracker<Stockpile> stockpileTracker,
    DefaultEntityTracker<BlockObject> blockObjectTracker,
    DefaultEntityTracker<Bot> botTracker
)
{

    public ReadOnlyList<DistrictCenter> AllDistrictCenters => districtCenterRegistry.AllDistrictCenters;

    public bool FindDistrictCenter([NotNullWhen(true)]out DistrictCenter? dc, DistrictCenter? preferred = null)
    {
        if (preferred)
        {
            dc = preferred!;
            return true;
        }

        foreach (var c in AllDistrictCenters)
        {
            if (c)
            {
                dc = c!;
                return true;
            }
        }

        dc = null;
        return false;
    }

    public IEnumerable<Stockpile> FindStockpiles(string? hasGoodId)
    {
        foreach (var storage in stockpileTracker.Entities)
        {
            if (hasGoodId is null || storage.Inventory.UnreservedAmountInStock(hasGoodId) > 0)
            {
                yield return storage;
            }
        }
    }

    public IEnumerable<BaseComponent> FindEntityToSpawnNearby(BaseComponent? preferred = null)
    {
        if (preferred)
        {
            yield return preferred!;
        }

        foreach (var dc in AllDistrictCenters)
        {
            yield return dc;
        }

        foreach (var b in beaverPopulation._beaverCollection._beavers)
        {
            yield return b;
        }

        foreach (var b in botTracker.Entities)
        {
            yield return b;
        }

        foreach (var e in blockObjectTracker.Entities)
        {
            var bo = e.GetComponent<BlockObject>();
            if (bo)
            {
                yield return bo;
            }
        }
    }

}
