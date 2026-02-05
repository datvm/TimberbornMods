namespace PowerCopy.Services;

public class ObjectListingService(
    EventBus eb,
    EntityBadgeService entityBadgeService,
    PowerCopyService powerCopyService
) : ILoadableSingleton
{

    readonly Dictionary<string, HashSet<EntityComponent>> buildingsByTemplates = [];
    readonly Dictionary<string, FrozenSet<Type>> duplicableTypesByTemplates = [];

    public void Load()
    {
        eb.Register(this);
    }

    readonly List<IDuplicable> cacheDuplicables = [];
    public bool HasAnyDuplicableTypes(BaseComponent component, IEnumerable<Type> expectingTypes)
    {
        var template = component.GetTemplateName();
        if (!duplicableTypesByTemplates.TryGetValue(template, out var types))
        {
            component.GetComponents(cacheDuplicables);
            types = duplicableTypesByTemplates[template] = [..cacheDuplicables.Select(d => d.GetType())];
            cacheDuplicables.Clear();
        }

        return types.Any(expectingTypes.Contains);
    }

    public IEnumerable<EntityComponent> QueryObjects(ObjectListingQuery query)
    {
        if (query.Components.Count == 0) { return []; }

        IEnumerable<EntityComponent> searching = [];

        if (query.TemplateName is not null)
        {
            searching = buildingsByTemplates.TryGetValue(query.TemplateName, out var templates)
                ? templates : [];
        }
        else
        {
            var comps = query.Components;

            foreach (var (_, grp) in buildingsByTemplates)
            {
                if (grp.Count == 0) { continue; }

                var first = grp.First();
                if (!HasAnyDuplicableTypes(first, comps))
                {
                    continue;                    
                }

                searching = searching.Concat(grp
                    .Where(q => powerCopyService.HasAnyValidDuplicables(q, comps)));
            }
        }

        searching = searching.Where(q => q != query.Source);

        if (query.InDistrict)
        {
            var d = query.InDistrict;

            return searching.Where(b =>
            {
                var db = b.GetComponent<DistrictBuilding>();
                return db && db.District == d;
            });
        }
        else
        {
            return searching;
        }
    }

    readonly List<IDuplicable> duplicables = [];
    public ObjectListingDetailedResult QueryDetailedObjects(ObjectListingQuery query)
    {
        return new([..QueryObjects(query)
            .GroupBy(GroupByFunc)
            .Select(g => new KeyValuePair<DistrictCenter?, ImmutableArray<ObjectListingDetailedEntry>>(
                g.Key,
                [.. g.Select(GetEntry).OrderBy(q => q.Name)]
            ))
            .OrderBy(q => q.Key?.DistrictName)
        ]);

        static DistrictCenter? GroupByFunc(EntityComponent b)
        {
            var db = b.GetComponent<DistrictBuilding>();
            return (db && db.District) ? db.District : null;
        }

        ObjectListingDetailedEntry GetEntry(EntityComponent b)
        {
            var name = entityBadgeService.GetEntityName(b);

            b.GetComponents(duplicables);
            var dups = duplicables
                .Where(d => query.Components.Contains(d.GetType()))
                .ToImmutableArray();
            duplicables.Clear();

            return new(b, name, dups);
        }
    }

    public int Count(in ObjectListingQuery query) => QueryObjects(query).Count();

    public bool CanCopy(in ObjectListingQuery query, BlockObject to)
    {
        var template = to.GetComponent<TemplateSpec>();

        return template is not null 
            && to.GetComponent<EntityComponent>() != query.Source
            && (query.TemplateName is null || query.TemplateName == template.TemplateName)
            && powerCopyService.HasAnyValidDuplicables(to, query.Components);
    }

    [OnEvent]
    public void OnEntityInitialized(EntityInitializedEvent e)
    {
        var template = e.Entity.GetComponent<TemplateSpec>();
        if (template is null) { return; }

        buildingsByTemplates.GetOrAdd(template.TemplateName).Add(e.Entity);
    }

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        var template = e.Entity.GetComponent<TemplateSpec>();
        if (template is null) { return; }

        if (buildingsByTemplates.TryGetValue(template.TemplateName, out var set))
        {
            set.Remove(e.Entity);
        }
    }
}
