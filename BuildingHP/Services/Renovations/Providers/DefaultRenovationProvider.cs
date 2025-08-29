
namespace BuildingHP.Services.Renovations.Providers;

public class DefaultRenovationProviderDependencies(
    RenovationDialogController RenovationDialogController,
    DefaultRenovationPanelFactory DefaultRenovationPanelFactory,
    ILoc T,
    DialogBoxShower DialogBoxShower,
    BuildingRenovationDependencies BuildingRenovationDependencies,
    ITimeTriggerFactory timeTriggerFactory
)
{
    public readonly RenovationDialogController RenovationDialogController = RenovationDialogController;
    public readonly DefaultRenovationPanelFactory DefaultRenovationPanelFactory = DefaultRenovationPanelFactory;
    public readonly ILoc T = T;
    public readonly DialogBoxShower DialogBoxShower = DialogBoxShower;
    public readonly BuildingRenovationDependencies BuildingRenovationDependencies = BuildingRenovationDependencies;
    public readonly ITimeTriggerFactory TimeTriggerFactory = timeTriggerFactory;
}

public abstract class DefaultRenovationProvider(DefaultRenovationProviderDependencies di) : IRenovationProvider
{
    public static readonly ComponentKey SaveKey = new("BuildingRenovationComponent");
    public static readonly PropertyKey<float> ProgressKey = new("Progress");
    public static readonly PropertyKey<bool> GoodAcquiredKey = new("IsGoodAcquired");
    public static readonly PropertyKey<int> PriorityKey = new("Priority");

    protected readonly DefaultRenovationProviderDependencies di = di;
    protected readonly ILoc t = di.T;

    protected bool NoCache { get; set; }

    protected DefaultRenovationPanel? Cache { get; set; }
    public abstract string Id { get; }
    public RenovationSpec RenovationSpec { get; set; } = null!;
    public BuildingRenovationComponent? CurrentComponent { get; protected set; }
    public DefaultRenovationPanel? CurrentElement { get; protected set; }

    public abstract string? CanRenovate(BuildingRenovationComponent building);
    protected string? ValidateActive(BuildingRenovationComponent building) => building.HasRenovation(Id) ? t.T("LV.BHP.AlreadyActive") : null;

    public VisualElement CreateUI(BuildingRenovationComponent building)
    {
        CurrentComponent = building;
        var el = NoCache ? null : Cache;
        el ??= CreateUIElement(building);
        CurrentElement = el;
        UpdateUI(el, building);

        if (!NoCache)
        {
            Cache = el;
        }

        return el;
    }

    protected virtual DefaultRenovationPanel CreateUIElement(BuildingRenovationComponent building)
    {
        var el = di.DefaultRenovationPanelFactory.Create(this);
        el.OnStartRenovationRequested += () => OnRenovationRequested(el);

        return el;
    }

    protected virtual void UpdateUI(VisualElement element, BuildingRenovationComponent building)
    {
        CurrentComponent = building;
        if (element is DefaultRenovationPanel panel)
        {
            panel.SetToComponent();
        }
    }

    protected virtual void OnRenovationRequested(DefaultRenovationPanel ve)
    {
        if (!CurrentComponent) { return; } // Should not happen

        var err = CanRenovate(CurrentComponent);
        if (err is not null)
        {
            throw new InvalidOperationException(err);
        }

        if (CreateRenovation(CurrentComponent, ve))
        {
            CloseDialog();
        }
    }

    protected virtual bool CreateRenovation(BuildingRenovationComponent building, DefaultRenovationPanel ve)
    {
        building.Renovate(new(building, RenovationSpec, di.BuildingRenovationDependencies), ve.Priority);
        return true;
    }

    public virtual void ClearUI(VisualElement? element)
    {
        if (element is DefaultRenovationPanel panel)
        {
            panel.CleanUp();
        }

        CurrentComponent = null;
        CurrentElement = null;
    }

    public void CloseDialog() => di.RenovationDialogController.CloseDialog();

    public virtual void Save(BuildingRenovationComponent comp, BuildingRenovation renovation, IEntitySaver saver)
    {
        if (renovation.IsDone) { return; }

        var s = saver.GetComponent(SaveKey);
        PerformSave(comp, renovation, s);
    }

    protected virtual void PerformSave(BuildingRenovationComponent comp, BuildingRenovation renovation, IObjectSaver s)
    {
        s.Set(GoodAcquiredKey, renovation.IsGoodAcquired);
        s.Set(ProgressKey, renovation.TimeTrigger?.Progress ?? 0);
        s.Set(PriorityKey, (int)comp.Priority);
    }

    public virtual void Load(BuildingRenovationComponent comp, IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s)) { return; }
        PerformLoad(comp, s);
    }

    protected virtual void PerformLoad(BuildingRenovationComponent comp, IObjectLoader s)
    {
        var reno = CreateRenovationFromSave(comp, s);
        var priority = s.Has(PriorityKey) ? (Priority)s.Get(PriorityKey) : Priority.Normal;

        comp.Renovate(reno, priority);
    }

    protected virtual BuildingRenovation CreateRenovationFromSave(BuildingRenovationComponent comp, IObjectLoader s)
    {
        var r = new BuildingRenovation(comp, RenovationSpec, di.BuildingRenovationDependencies);
        LoadRenovationProgress(r, comp, s);

        return r;
    }

    protected virtual void LoadRenovationProgress(BuildingRenovation r, BuildingRenovationComponent comp, IObjectLoader s)
    {
        if (s.Has(GoodAcquiredKey) && s.Get(GoodAcquiredKey))
        {
            var trigger = comp.RenovationService.CreateRenovationTriggerIfNeeded(r);
            if (trigger is not null && s.Has(ProgressKey))
            {
                trigger.FastForwardProgress(s.Get(ProgressKey));
            }

            r.OnGoodAcquired(trigger);
        }
    }

}
