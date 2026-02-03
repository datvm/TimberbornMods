namespace PowerCopy.Services;

public class ObjectListingService(EventBus eb) : ILoadableSingleton
{

    readonly Dictionary<string, HashSet<EntityComponent>> buildingsByTemplates = [];

    public void Load()
    {
        eb.Register(this);
    }

    static bool EntityHasAnyComponent(EntityComponent entity, IReadOnlyList<Type> types)
    {
        var map = entity._componentCache._typeIndexMap._typeIndex;
        for (int i = 0; i < types.Count; i++)
        {
            if (map.ContainsKey(types[i]))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<EntityComponent> QueryObjects(ObjectListingQuery query)
    {
        var searching = query.TemplateName is not null
            ? (buildingsByTemplates.TryGetValue(query.TemplateName, out var templates) ? templates : [])
            : buildingsByTemplates.Values.SelectMany(t => t);

        searching = searching.Where(q => q != query.Source);

        if (query.SelectedBuildings is not null)
        {
            searching = searching.Where(query.SelectedBuildings.Contains);
        }

        if (query.Components is not null)
        {
            var comps = query.Components;
            searching = searching.Where(b => EntityHasAnyComponent(b, comps));
        }

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

    public int Count(ObjectListingQuery query) => QueryObjects(query).Count();

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
