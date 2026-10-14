namespace WarningsBeGone.Services;

[BindSingleton]
public class StatusHidingService(
    ISingletonLoader loader,
    EntityRegistry entityRegistry
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(StatusHidingService));
    static readonly PropertyKey<string> GlobalHidingStatusesKey = new("GlobalHidingStatuses");
    static readonly PropertyKey<string> TemplateHidingStatusesKey = new("TemplateHidingStatuses");
    static readonly PropertyKey<bool> HideCornerWarningsKey = new("HideCornerWarnings");
    static readonly Dictionary<string, int> hiddenDescriptionCounts = [];
    static readonly HashSet<string> globallyHidden = [];

    readonly HashSet<string> globalHidingStatuses = [];
    readonly Dictionary<string, HashSet<string>> templateHidingStatuses = [];
    int hiddenPriorityCount;

    public IReadOnlyCollection<string> GlobalHidingStatuses => globalHidingStatuses;

    public IReadOnlyCollection<string> GetHidingStatusesForTemplate(string template) =>
        templateHidingStatuses.TryGetValue(template, out var statuses) ? statuses : [];

    public static bool HideCornerWarnings { get; private set; }
    public static bool AnyPriorityHidden { get; private set; }

    public static bool IsDescriptionHiddenAnywhere(string status) => hiddenDescriptionCounts.ContainsKey(status);

    public static bool IsHiddenGlobally(string status) => globallyHidden.Contains(status);

    public bool Affects(string template) =>
        globalHidingStatuses.Count > 0
        || (templateHidingStatuses.TryGetValue(template, out var statuses) && statuses.Count > 0);

    public void Load()
    {
        HideCornerWarnings = false;
        AnyPriorityHidden = false;
        hiddenPriorityCount = 0;
        hiddenDescriptionCounts.Clear();
        globallyHidden.Clear();
        globalHidingStatuses.Clear();
        templateHidingStatuses.Clear();
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(GlobalHidingStatusesKey))
        {
            var loaded = JsonConvert.DeserializeObject<IEnumerable<string>>(s.Get(GlobalHidingStatusesKey)) ?? [];
            foreach (var status in loaded)
            {
                RememberGlobal(status);
            }
        }

        if (s.Has(TemplateHidingStatusesKey))
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(s.Get(TemplateHidingStatusesKey)) ?? [];
            foreach (var (template, statuses) in dict)
            {
                templateHidingStatuses[template] = statuses;
                foreach (var status in statuses)
                {
                    AddHiddenDescription(status);
                }
            }
        }

        if (s.Has(HideCornerWarningsKey))
        {
            HideCornerWarnings = s.Get(HideCornerWarningsKey);
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

        if (HideCornerWarnings)
        {
            s.Set(HideCornerWarningsKey, true);
        }
    }

    public bool IsStatusHiddenGlobally(string status) => globalHidingStatuses.Contains(status);

    public bool ShouldHide(string status, string template) =>
        globalHidingStatuses.Contains(status)
            || (templateHidingStatuses.TryGetValue(template, out var statuses) && statuses.Contains(status));

    public void AddHiddenDescription(string status)
    {
        hiddenDescriptionCounts.TryGetValue(status, out var count);
        hiddenDescriptionCounts[status] = count + 1;
    }

    public void RemoveHiddenDescription(string status)
    {
        if (!hiddenDescriptionCounts.TryGetValue(status, out var count)) { return; }

        if (count <= 1)
        {
            hiddenDescriptionCounts.Remove(status);
        }
        else
        {
            hiddenDescriptionCounts[status] = count - 1;
        }
    }

    public void SetHiddenPriorityContribution(bool contributing)
    {
        if (contributing)
        {
            hiddenPriorityCount++;
        }
        else if (hiddenPriorityCount > 0)
        {
            hiddenPriorityCount--;
        }

        AnyPriorityHidden = hiddenPriorityCount > 0;
    }

    public void ToggleGlobalStatusHiding(string status, bool shouldHide)
    {
        if (shouldHide)
        {
            RememberGlobal(status);
        }
        else
        {
            ForgetGlobal(status);
        }

        RefreshStatusSubject(null);
    }

    public void ToggleTemplateStatusHiding(string template, string status, bool shouldHide)
    {
        var list = templateHidingStatuses.GetOrAdd(template, () => []);

        if (shouldHide)
        {
            if (list.Add(status))
            {
                AddHiddenDescription(status);
            }
        }
        else if (list.Remove(status))
        {
            RemoveHiddenDescription(status);
        }

        RefreshStatusSubject(template);
    }

    public void ToggleHideCornerWarnings(bool hide)
    {
        HideCornerWarnings = hide;
        RefreshStatusSubject(null);
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

    void RememberGlobal(string status)
    {
        if (!globalHidingStatuses.Add(status)) { return; }

        globallyHidden.Add(status);
        AddHiddenDescription(status);
    }

    void ForgetGlobal(string status)
    {
        if (!globalHidingStatuses.Remove(status)) { return; }

        globallyHidden.Remove(status);
        RemoveHiddenDescription(status);
    }

}
