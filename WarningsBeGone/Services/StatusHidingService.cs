namespace WarningsBeGone.Services;

public class StatusHidingService(
    ISingletonLoader loader,
    EntityRegistry entityRegistry
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(StatusHidingService));
    static readonly PropertyKey<string> GlobalHidingStatusesKey = new("GlobalHidingStatuses");
    static readonly PropertyKey<string> TemplateHidingStatusesKey = new("TemplateHidingStatuses");

    readonly HashSet<string> globalHidingStatuses = [];
    readonly Dictionary<string, HashSet<string>> templateHidingStatuses = [];

    public IReadOnlyCollection<string> GlobalHidingStatuses => globalHidingStatuses;

    public IReadOnlyCollection<string> GetHidingStatusesForTemplate(string template) =>
        templateHidingStatuses.TryGetValue(template, out var statuses) ? statuses : [];

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(GlobalHidingStatusesKey))
        {
            globalHidingStatuses.UnionWith(JsonConvert.DeserializeObject<IEnumerable<string>>(s.Get(GlobalHidingStatusesKey)));
        }

        if (s.Has(TemplateHidingStatusesKey))
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(s.Get(TemplateHidingStatusesKey)) ?? [];
            foreach (var (k, v) in dict)
            {
                templateHidingStatuses[k] = v;
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        // Cleanup empty entries
        var emptyTemplates = templateHidingStatuses
            .Where(kvp => kvp.Value.Count == 0)
            .Select(kvp => kvp.Key)
            .ToArray();
        foreach (var template in emptyTemplates)
        {
            templateHidingStatuses.Remove(template);
        }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(GlobalHidingStatusesKey, JsonConvert.SerializeObject(globalHidingStatuses));
        s.Set(TemplateHidingStatusesKey, JsonConvert.SerializeObject(templateHidingStatuses));
    }

    public bool IsStatusHiddenGlobally(string status) => globalHidingStatuses.Contains(status);

    public bool ShouldHide(string status, string template) =>
        globalHidingStatuses.Contains(status)
            || (templateHidingStatuses.TryGetValue(template, out var statuses) && statuses.Contains(status));

    public void ToggleGlobalStatusHiding(string status, bool shouldHide)
    {
        if (shouldHide)
        {
            globalHidingStatuses.Add(status);
        }
        else
        {
            globalHidingStatuses.Remove(status);
        }

        RefreshStatusSubject(null);
    }

    public void ToggleTemplateStatusHiding(string template, string status, bool shouldHide)
    {
        var list = templateHidingStatuses.GetOrAdd(template, () => []);

        if (shouldHide)
        {
            list.Add(status);
        }
        else
        {
            list.Remove(status);
        }

        RefreshStatusSubject(template);
    }

    public void RefreshStatusSubject(string? filterTemplate)
    {
        foreach (var e in entityRegistry.Entities)
        {
            var statusHiding = e.GetComponent<StatusHidingComponent>();
            if (statusHiding is null ||
                (filterTemplate is not null && statusHiding.TemplateName != filterTemplate))
            {
                continue;
            }

            statusHiding.RefreshStatusSubject();
        }
    }

}
