namespace PowerCopy.Services;

[BindSingleton(Contexts = BindAttributeContext.NonMenu)]
public class ObjectListingService(
    EventBus eb,
    BuildingSettingsResolver buildingSettingsResolver,
    ILoc t
) : ILoadableSingleton
{

    readonly Dictionary<string, HashSet<EntityComponent>> buildingsByTemplates = [];

    public void Load()
    {
        eb.Register(this);
    }

    public IEnumerable<EntityComponent> QueryObjects(ObjectListingQuery query)
    {
        if (query.Settings.Count == 0) { return []; }

        IEnumerable<EntityComponent> searching = [];

        if (query.TemplateName is not null)
        {
            // If query by template, no need to check for types
            searching = buildingsByTemplates.TryGetValue(query.TemplateName, out var templates)
                ? templates : [];
        }
        else
        {
            var comps = query.Settings;

            foreach (var (_, grp) in buildingsByTemplates)
            {
                if (grp.Count == 0) { continue; }

                // First, determine if any of the settings matches
                var first = grp.First();
                var settings = buildingSettingsResolver.GetSettingsForObject(first);
                if (!settings.Any(comps.Contains)) { continue; }

                // Then, only add if there is actually a setting that can be copied
                var matches = grp
                    .Where(e => CanCopy(e, settings));

                searching = searching.Concat(matches);
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

    public ObjectListingDetailedResult QueryDetailedObjects(ObjectListingQuery query)
    {
        return new([..QueryObjects(query)
            .GroupBy(GroupByFunc)
            .Select(g => (
                g.Key,
                g.Select(GetEntry).OrderBy(q => q.Name).ToArray()
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
            var name = b.GetName(t);
            var pairs = buildingSettingsResolver.Get(b, query.Settings);

            return new(b, name, pairs);
        }
    }

    public int Count(in ObjectListingQuery query) => QueryObjects(query).Count();

    public bool CanCopy(in ObjectListingQuery query, BlockObject to)
    {
        var template = to.GetComponent<TemplateSpec>();

        return template is not null
            && to.GetComponent<EntityComponent>() != query.Source
            && (query.TemplateName is null || query.TemplateName == template.TemplateName)
            && CanCopy(to, query.Settings);
    }

    bool CanCopy(BaseComponent target, IEnumerable<IBuildingSettings> settings)
        => settings.Any(s => s.CanDeserialize(s.GetComponent(target)));

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
