namespace WarningsBeGone.Components;

public class StatusHidingComponent(StatusHidingService statusHidingRepo) : BaseComponent, IPersistentEntity, IAwakableComponent, IInitializableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(StatusHidingComponent));
    static readonly ListKey<string> HidingStatusesKey = new("HidingStatuses");

    readonly HashSet<string> hidingStatuses = [];
    readonly HashSet<StatusInstance> hiddenActive = [];
    public string TemplateName { get; private set; } = "";
    public string DisplayName { get; private set; } = "";

#nullable disable
    StatusSubject statusSubject;
#nullable enable

    StatusIconCycler? statusIconCycler;

    public ReadOnlyList<StatusInstance> GetFilteredActiveStatuses()
    {
        List<StatusInstance> result = [];

        // First, check priority
        foreach (var s in statusSubject._activePriorityStatuses)
        {
            if (!ShouldHide(s.StatusDescription))
            {
                result.Add(s);
            }
        }

        // Then if empty, check normal
        if (result.Count == 0)
        {
            foreach (var s in statusSubject._activeNormalStatuses)
            {
                if (!ShouldHide(s.StatusDescription))
                {
                    result.Add(s);
                }
            }
        }

        return new(result);
    }

    public bool IsFilteredInPriorityMode()
    {
        foreach (var s in statusSubject._activePriorityStatuses)
        {
            if (!ShouldHide(s.StatusDescription))
            {
                return true;
            }
        }
        return false;
    }

    ReadOnlyList<StatusInstance> GetUnfilteredActiveStatuses() =>
        new([
            ..statusSubject._activePriorityStatuses,
            .. statusSubject._activeNormalStatuses,
            .. hiddenActive,
        ]);

    public void InitializeEntity() => RefreshStatusSubject();

    public void OnStatusUpdated(StatusInstance status, bool isActive)
    {
        if (!isActive)
        {
            hiddenActive.Remove(status);
        }
        else if (statusSubject is not null && ShouldHide(status.StatusDescription))
        {
            RefreshStatusSubject();
        }
    }

    public void Awake()
    {
        statusSubject = GetComponent<StatusSubject>();
        statusIconCycler = GetComponent<StatusIconCycler>();

        var template = GetComponent<TemplateSpec>();
        TemplateName = template.TemplateName;
        DisplayName = GetComponent<LabeledEntity>()?.DisplayName ?? TemplateName;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        hidingStatuses.UnionWith(s.Get(HidingStatusesKey));
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (hidingStatuses.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(HidingStatusesKey, hidingStatuses);
    }

    public bool ShouldHide(string status)
        => hidingStatuses.Contains(status) || statusHidingRepo.ShouldHide(status, TemplateName);

    public bool IsStatusSelfHiding(string status) => hidingStatuses.Contains(status);
    public void ToggleHiding(string status, bool shouldHide)
    {
        if (shouldHide)
        {
            hidingStatuses.Add(status);
        }
        else
        {
            hidingStatuses.Remove(status);
        }
        RefreshStatusSubject();
    }

    public string[] GatherStatuses()
    {
        HashSet<string> statuses = [];

        foreach (var s in GetUnfilteredActiveStatuses())
        {
            statuses.Add(s.StatusDescription);
        }

        foreach (var s in hidingStatuses)
        {
            statuses.Add(s);
        }

        foreach (var s in statusHidingRepo.GetHidingStatusesForTemplate(TemplateName))
        {
            statuses.Add(s);
        }

        return [.. statuses.OrderBy(q => q)];
    }

    public void RefreshStatusSubject()
    {
        foreach (var s in GetUnfilteredActiveStatuses())
        {
            var hide = ShouldHide(s.StatusDescription);
            var list = s.IsPriorityStatus ? statusSubject._activePriorityStatuses : statusSubject._activeNormalStatuses;
            if (hide && hiddenActive.Add(s)) { list.Remove(s); }
            else if (!hide && hiddenActive.Remove(s)) { list.Add(s); }

            if (hide && StatusHidingService.HideCornerWarnings && s.StatusValueGetter is null) { s.Deactivate(); }
            else { s.Activate(); }
        }

        if (statusIconCycler)
        {
            statusIconCycler!.OnStatusToggled(null!, null!);
        }
    }

}
