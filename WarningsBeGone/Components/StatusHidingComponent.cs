namespace WarningsBeGone.Components;

[AddTemplateModule2(typeof(StatusSubject))]
public class StatusHidingComponent(StatusHidingService statusHidingRepo) : BaseComponent, IPersistentEntity, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(StatusHidingComponent));
    static readonly ListKey<string> HidingStatusesKey = new("HidingStatuses");

    readonly HashSet<string> hidingStatuses = [];
    readonly List<StatusInstance> filteredActive = [];
    ReadOnlyList<StatusInstance> filteredActiveView;
    bool filteredDirty = true;
    bool cachedInPriorityMode;
    bool reportsHiddenPriority;
    readonly HashSet<StatusInstance> hiddenActive = [];

    public string TemplateName { get; private set; } = "";
    public string DisplayName { get; private set; } = "";
    public bool NeedsFilter { get; private set; }

#nullable disable
    StatusSubject statusSubject;
#nullable enable

    StatusIconCycler? statusIconCycler;

    public ReadOnlyList<StatusInstance> FilteredActiveStatuses
    {
        get
        {
            EnsureFiltered();
            return filteredActiveView;
        }
    }

    public bool CachedInPriorityMode
    {
        get
        {
            EnsureFiltered();
            return cachedInPriorityMode;
        }
    }

    public void Awake()
    {
        filteredActiveView = new(filteredActive);
        statusSubject = GetComponent<StatusSubject>();
        statusIconCycler = GetComponent<StatusIconCycler>();

        var template = GetComponent<TemplateSpec>();
        TemplateName = template.TemplateName;
        DisplayName = GetComponent<LabeledEntity>()?.DisplayName ?? TemplateName;

        statusSubject.StatusToggled += OnStatusToggled;
        RecomputeNeedsFilter();
    }

    public void InitializeEntity()
    {
        RecomputeNeedsFilter();
        filteredDirty = true;
        RefreshPriorityContribution();
        RefreshStatusSubject();
    }

    public void DeleteEntity()
    {
        statusSubject?.StatusToggled -= OnStatusToggled;

        foreach (var status in hidingStatuses)
        {
            statusHidingRepo.RemoveHiddenDescription(status);
        }

        ApplyPriorityContribution(false);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        foreach (var status in s.Get(HidingStatusesKey).Select(statusHidingRepo.FromKey))
        {
            if (!hidingStatuses.Add(status)) { continue; }

            statusHidingRepo.AddHiddenDescription(status);
        }

        RecomputeNeedsFilter();
        filteredDirty = true;
        RefreshPriorityContribution();
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (hidingStatuses.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(HidingStatusesKey, [.. hidingStatuses.Select(statusHidingRepo.ToKey)]);
    }

    public bool ShouldHide(string status)
        => hidingStatuses.Contains(status) || statusHidingRepo.ShouldHide(status, TemplateName);

    public bool IsStatusSelfHiding(string status) => hidingStatuses.Contains(status);

    public void ToggleHiding(string status, bool shouldHide)
    {
        if (shouldHide)
        {
            if (hidingStatuses.Add(status))
            {
                statusHidingRepo.AddHiddenDescription(status);
            }
        }
        else if (hidingStatuses.Remove(status))
        {
            statusHidingRepo.RemoveHiddenDescription(status);
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
        RecomputeNeedsFilter();
        filteredDirty = true;
        RefreshPriorityContribution();

        foreach (var s in GetUnfilteredActiveStatuses())
        {
            var hide = NeedsFilter && ShouldHide(s.StatusDescription);
            var list = s.IsPriorityStatus ? statusSubject._activePriorityStatuses : statusSubject._activeNormalStatuses;
            if (hide && hiddenActive.Add(s)) { list.Remove(s); }
            else if (!hide && hiddenActive.Remove(s)) { list.Add(s); }
            s.IsActive = !hide || !StatusHidingService.HideCornerWarnings || s.StatusValueGetter is not null;
        }

        if (statusIconCycler)
        {
            statusIconCycler!.OnStatusToggled(null!, null!);
        }
    }

    void OnStatusToggled(object sender, StatusInstance status)
    {
        if (!status.IsActive)
        {
            hiddenActive.Remove(status);
        }
        else if (NeedsFilter && ShouldHide(status.StatusDescription))
        {
            RefreshStatusSubject();
        }

        filteredDirty = true;
        if (reportsHiddenPriority || status is { IsPriorityStatus: true })
        {
            RefreshPriorityContribution();
        }
    }

    void RecomputeNeedsFilter()
    {
        NeedsFilter = hidingStatuses.Count > 0 || statusHidingRepo.Affects(TemplateName);
    }

    void RefreshPriorityContribution()
    {
        if (statusSubject is null) { return; }

        var hidden = false;
        foreach (var s in statusSubject._activePriorityStatuses)
        {
            if (!ShouldHide(s.StatusDescription)) { continue; }

            hidden = true;
            break;
        }

        ApplyPriorityContribution(hidden);
    }

    void ApplyPriorityContribution(bool hidden)
    {
        if (hidden == reportsHiddenPriority) { return; }

        reportsHiddenPriority = hidden;
        statusHidingRepo.SetHiddenPriorityContribution(hidden);
    }

    void EnsureFiltered()
    {
        if (!filteredDirty) { return; }

        filteredActive.Clear();
        var anyHiddenPriority = false;
        var anyVisiblePriority = false;

        foreach (var s in statusSubject._activePriorityStatuses)
        {
            if (ShouldHide(s.StatusDescription))
            {
                anyHiddenPriority = true;
                continue;
            }

            filteredActive.Add(s);
            anyVisiblePriority = true;
        }

        cachedInPriorityMode = anyVisiblePriority;

        if (!anyVisiblePriority)
        {
            foreach (var s in statusSubject._activeNormalStatuses)
            {
                if (ShouldHide(s.StatusDescription)) { continue; }

                filteredActive.Add(s);
            }
        }

        filteredDirty = false;
        ApplyPriorityContribution(anyHiddenPriority);
    }

    ReadOnlyList<StatusInstance> GetUnfilteredActiveStatuses() =>
        new([
            ..statusSubject._activePriorityStatuses,
            .. statusSubject._activeNormalStatuses,
            .. hiddenActive,
        ]);

}
