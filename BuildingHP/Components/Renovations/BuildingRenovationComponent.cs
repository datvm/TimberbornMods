namespace BuildingHP.Components.Renovations;

public class BuildingRenovationComponent : BaseComponent, IPersistentEntity, IFinishedStateListener
{

    static readonly PropertyKey<string> CompletedRenovationsKey = new("RenovationsCompleted");
    static readonly ListKey<string> ActiveRenovationsKey = new("RenovationsActive");
    static readonly PropertyKey<string> CurrentRenovationIdKey = new("CurrentRenovationId");

#nullable disable
    public BuildingHPComponent BuildingHPComponent { get; private set; }
    StatusToggle lackOfResourcesStatus;
    ILoc t;
#nullable enable
    public BuildingRenovationService RenovationService => BuildingHPComponent.BuildingHPService.BuildingRenovationService;

    public BuildingRenovation? CurrentRenovation { get; private set; }
    public Priority Priority { get; private set; }
    public bool CanRenovate => CurrentRenovation is null;
    public bool IsRenovating => CurrentRenovation is not null && !CurrentRenovation.IsDone;
    public bool CanChangePriority => CurrentRenovation is not null && !CurrentRenovation.IsGoodAcquired;

    public Dictionary<string, List<CompletedRenovation>> CompletedRenovations { get; private set; } = [];
    public IList<CompletedRenovation> AllCompletedRenovations => [..CompletedRenovations.Values.SelectMany(v => v).OrderByDescending(q => q.Time)];

    public HashSet<string> ActiveRenovations { get; private set; } = [];
    public bool CanCancel => CurrentRenovation is not null && CurrentRenovation.CanCancel;

    public event Action<BuildingRenovation>? RenovationStarted;
    public event Action<BuildingRenovation>? RenovationDone;
    public event Action<BuildingRenovation>? RenovationCompleted;

    public bool HasRenovation(string id, bool active = true)
        => active
        ? ActiveRenovations.Contains(id)
        : CompletedRenovations.TryGetValue(id, out var list) && list.Count > 0;

    [Inject]
    public void Inject(ILoc t)
    {
        this.t = t;
    }

    public void Awake()
    {
        BuildingHPComponent = this.GetHPComponent();

        lackOfResourcesStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon("LackOfResources", t.T("LV.BHP.NoMaterialShort"), t.T("Status.ConstructionSites.NoMaterials"), 1f);
    }

    public void Start()
    {
        GetComponentFast<StatusSubject>().RegisterStatus(lackOfResourcesStatus);
    }

    public void Renovate(BuildingRenovation r, Priority priority)
    {
        if (!CanRenovate)
        {
            throw new InvalidOperationException("Already renovating");
        }

        r.GoodAcquireFailed += lackOfResourcesStatus.Activate;
        r.GoodAcquired += lackOfResourcesStatus.Deactivate;
        r.Done += () => OnRenovationDone(r);

        CurrentRenovation = r;
        Priority = priority;
        RenovationService.RegisterRenovation(r, priority);
        RenovationStarted?.Invoke(r);
    }

    void OnRenovationDone(BuildingRenovation r)
    {
        if (CurrentRenovation != r) { return; }

        lackOfResourcesStatus.Deactivate();
        CurrentRenovation = null;

        if (r.IsCompleted)
        {
            RenovationCompleted?.Invoke(r);
        }

        RenovationDone?.Invoke(r);
    }

    public void ChangePriority(Priority priority)
    {
        if (CurrentRenovation is null)
        {
            throw new InvalidOperationException("Not renovating");
        }

        if (CurrentRenovation.IsGoodAcquired)
        {
            throw new InvalidOperationException("Cannot change priority after materials are acquired");
        }

        Priority = priority;
        RenovationService.RegisterRenovation(CurrentRenovation, priority);
    }

    public void AddCompletedRenovation(string id, bool addToActive)
    {
        var renovationRecord = new CompletedRenovation(id, RenovationService.PartialDayNumber);

        if (!CompletedRenovations.TryGetValue(id, out var list))
        {
            CompletedRenovations[id] = list = [];
        }
        list.Add(renovationRecord);

        if (addToActive)
        {
            ActiveRenovations.Add(id);
        }
    }

    public void RemoveActiveRenovation(string id)
    {
        ActiveRenovations.Remove(id);
    }

    public void RequestCancelRenovation()
    {
        if (!CanCancel)
        {
            throw new InvalidOperationException("Cannot cancel renovation");
        }
        CurrentRenovation!.RequestCancel();
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(BuildingHPComponent.SaveKey);
        s.Set(CompletedRenovationsKey, JsonConvert.SerializeObject(CompletedRenovations));
        s.Set(ActiveRenovationsKey, ActiveRenovations);

        if (CurrentRenovation is not null)
        {
            var id = CurrentRenovation.Id;
            s.Set(CurrentRenovationIdKey, id);

            RenovationService.RenovationRegistry.Providers[id].Save(this, CurrentRenovation, entitySaver);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(BuildingHPComponent.SaveKey, out var s)) { return; }

        if (s.Has(CompletedRenovationsKey))
        {
            var json = s.Get(CompletedRenovationsKey);
            CompletedRenovations = JsonConvert.DeserializeObject<Dictionary<string, List<CompletedRenovation>>>(json) ?? [];
        }

        if (s.Has(ActiveRenovationsKey))
        {
            ActiveRenovations = [.. s.Get(ActiveRenovationsKey)];
        }

        if (s.Has(CurrentRenovationIdKey))
        {
            var id = s.Get(CurrentRenovationIdKey);
            if (RenovationService.RenovationRegistry.Providers.TryGetValue(id, out var p))
            {
                p.Load(this, entityLoader);
            }
        }
    }

    public void OnEnterFinishedState()
    {
    }

    public void OnExitFinishedState()
    {
        if (CurrentRenovation is null || CurrentRenovation.IsDone) { return; }

        CurrentRenovation.PerformCancel();
        CurrentRenovation = null;
    }
}

public record CompletedRenovation(string Id, float Time);