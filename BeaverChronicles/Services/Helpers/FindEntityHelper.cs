namespace BeaverChronicles.Services.Helpers;

[BindSingleton]
public class FindEntityHelper(
    DistrictCenterRegistry districtCenterRegistry,
    BeaverPopulation beaverPopulation,
    DefaultEntityTracker<Stockpile> stockpileTracker,
    DefaultEntityTracker<BlockObject> blockObjectTracker,
    DefaultEntityTracker<BlockObjectBound> blockObjectBoundTracker,
    DefaultEntityTracker<Bot> bots,
    EntityRegistry entities,
    EntityByTemplateTracker entityByTemplateTracker
)
{

    public ReadOnlyList<DistrictCenter> AllDistrictCenters => districtCenterRegistry.AllDistrictCenters;

    public bool TryFindEntity(string id, [NotNullWhen(true)] out EntityComponent? entity)
    {
        entity = null;
        if (!Guid.TryParse(id, out var g)) { return false; }

        entity = entities.GetEntity(g);
        if (!entity)
        {
            entity = null;
            return false;
        }

        return true;
    }

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

    public IEnumerable<BaseComponent> GetCharacters(
        CharacterType types,
        IReadOnlyList<BoundsInt> areas)
    {
        if (areas.Count == 0)
        {
            return GetCharacters(types);
        }

        return GetCharacters(types).Where(c => areas.FastAny(a => a.Contains(c.Transform.position.FloorToInt())));
    }

    public IEnumerable<BlockObjectBound> FindBuildings(
        IReadOnlyCollection<string>? templateNames = null,
        IReadOnlyList<string>? templatePrefixes = null,
        IReadOnlyList<BoundsInt>? areas = null,
        AreaCondition areaCondition = AreaCondition.Intersects)
    {
        foreach (var bound in blockObjectBoundTracker.Entities)
        {
            if (!MatchesTemplate(bound, templateNames, templatePrefixes)) { continue; }
            if (!MatchesAreas(bound, areas, areaCondition)) { continue; }

            yield return bound;
        }
    }

    public IEnumerable<TemplateTrackerComponent> FindEntitiesByTemplates(
        IReadOnlyCollection<string>? templateNames = null,
        IReadOnlyList<string>? templatePrefixes = null)
    {
        var templates = GetTrackedTemplateNames(templateNames, templatePrefixes);
        if (templates.Count == 0) { yield break; }

        foreach (var template in templates)
        {
            foreach (var e in entityByTemplateTracker.GetEntitiesByTemplate(template))
            {
                yield return e;
            }
        }
    }

    public IEnumerable<TemplateTrackerComponent> FindEntitiesByTemplates(
        IReadOnlyCollection<string>? templateNames,
        IReadOnlyList<string>? templatePrefixes,
        IReadOnlyList<BoundsInt>? areas,
        AreaCondition areaCondition = AreaCondition.Intersects)
    {
        if (areas is not { Count: > 0 })
        {
            return FindEntitiesByTemplates(templateNames, templatePrefixes);
        }

        return FindEntitiesByTemplates(templateNames, templatePrefixes)
            .Where(e => e.GetComponent<BlockObjectBound>() is { } bound
                && MatchesAreas(bound, areas, areaCondition));
    }

    public IEnumerable<T> FindEntitiesByTemplates<T>(
        IReadOnlyCollection<string>? templateNames = null,
        IReadOnlyList<string>? templatePrefixes = null)
    {
        foreach (var entity in FindEntitiesByTemplates(templateNames, templatePrefixes))
        {
            var component = entity.GetComponent<T>();
            if (component is not null)
            {
                yield return component;
            }
        }
    }

    public HashSet<string> GetTrackedTemplateNames(
        IReadOnlyCollection<string>? templateNames = null,
        IReadOnlyList<string>? templatePrefixes = null)
    {
        var hasPrefix = templatePrefixes is { Count: > 0 };
        var hasNames = templateNames is { Count: > 0 };

        if (!hasPrefix && !hasNames) { return []; }

        HashSet<string> result = [];
        var keys = entityByTemplateTracker.TrackedTemplates;

        if (hasNames)
        {
            foreach (var n in templateNames!)
            {
                if (keys.Contains(n))
                {
                    result.Add(n);
                }
            }
        }

        if (hasPrefix)
        {
            foreach (var k in keys)
            {
                if (templatePrefixes!.FastAny(p => k.StartsWith(p)))
                {
                    result.Add(k);
                }
            }
        }

        return result;
    }

    static bool MatchesTemplate(
        BlockObjectBound bound,
        IReadOnlyCollection<string>? templateNames,
        IReadOnlyList<string>? templatePrefixes)
    {
        var hasNames = templateNames is { Count: > 0 };
        var hasPrefixes = templatePrefixes is { Count: > 0 };
        if (!hasNames && !hasPrefixes) { return true; }

        var templateName = bound.GetTemplateName();
        if (hasNames && !templateNames!.Contains(templateName)) { return false; }
        if (hasPrefixes && !templatePrefixes!.FastAny(p => templateName.StartsWith(p))) { return false; }

        return true;
    }

    static bool MatchesAreas(
        BlockObjectBound bound,
        IReadOnlyList<BoundsInt>? areas,
        AreaCondition areaCondition)
    {
        if (areas is not { Count: > 0 }) { return true; }

        var buildingBounds = bound.Bounds;
        return ConditionType.Any.Evaluate(areas, a => areaCondition.Evaluate(buildingBounds, a));
    }

}
