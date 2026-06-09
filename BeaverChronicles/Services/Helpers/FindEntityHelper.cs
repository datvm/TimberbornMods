namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class FindEntityHelper(
    DistrictCenterRegistry districtCenterRegistry,
    BeaverPopulation beaverPopulation,
    DefaultEntityTracker<Stockpile> stockpileTracker,
    DefaultEntityTracker<BlockObject> blockObjectTracker,
    DefaultEntityTracker<Bot> bots,
    EntityRegistry entities
)
{

    public ReadOnlyList<DistrictCenter> AllDistrictCenters => districtCenterRegistry.AllDistrictCenters;

    public bool FindDistrictCenter([NotNullWhen(true)] out DistrictCenter? dc, DistrictCenter? preferred = null)
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

    public int CountFinishedBlockObjects(IReadOnlyCollection<string>? templateNames, IReadOnlyCollection<string>? excluded)
    {
        var counter = 0;

        foreach (var bo in blockObjectTracker.Entities)
        {
            if (!bo.IsFinished) { continue; }

            var templateName = bo.GetTemplateName();
            var satisfy = templateNames is null || templateNames.Contains(templateName);
            if (satisfy)
            {
                if (excluded is not null && excluded.Contains(templateName))
                {
                    continue;
                }

                counter++;
            }
        }

        return counter;
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

        foreach (var b in bots.Entities)
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

    public DistrictCenter? FindDistrictCenter(string? guid)
    {
        if (!Guid.TryParse(guid, out var g)) { return null; }

        var entity = entities.GetEntity(g);
        if (!entity) { return null; }

        return entity.GetComponentOrNull<DistrictCenter>();
    }

    public int FindInAreas(IReadOnlyList<Bounds> areas, CharacterType characterTypes = BeaverChroniclesUtils.AllCharactersEnum, int stopAt = 1)
    {
        var counter = 0;

        foreach (var c in GetCharacters(characterTypes))
        {
            var pos = c.Transform.position;

            if (areas.FastAny(a => a.Contains(pos)))
            {
                counter++;
                if (counter >= stopAt) { return counter; }
            }
        }

        return counter;
    }

    public IEnumerable<BaseComponent> GetCharacters(CharacterType types = BeaverChroniclesUtils.AllCharactersEnum)
    {
        if ((types & CharacterType.AdultBeaver) != 0)
        {
            foreach (var b in beaverPopulation._beaverCollection._adults)
            {
                yield return b;
            }
        }

        if ((types & CharacterType.ChildBeaver) != 0)
        {
            foreach (var b in beaverPopulation._beaverCollection._children)
            {
                yield return b;
            }
        }

        if ((types & CharacterType.Bot) != 0)
        {
            foreach (var b in bots.Entities)
            {
                yield return b;
            }
        }
    }

}
