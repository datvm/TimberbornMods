namespace BuildingRenovations.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingRenovationComponent(BuildingRenovationService service)
    : BaseComponent, IPersistentEntity, IFinishedStateListener, IAwakableComponent, IPostLoadableEntity
{
    public static readonly ComponentKey SaveKey = new(nameof(BuildingRenovationComponent));
    static readonly ListKey<string> ActiveRenovationsKey = new("RenovationsActive");
    static readonly PropertyKey<string> CurrentRenovationIdKey = new("CurrentRenovationId");
    static readonly PropertyKey<bool> MaterialsAcquiredKey = new("MaterialsAcquired");
    static readonly PropertyKey<int> PriorityKey = new("Priority");

#nullable disable
    StatusToggle lackOfResourcesStatus;
    BlockObject blockObject;
    RenovationDistroReceiver distro;
    RenovationWorkComponent work;
    RenovationRecordComponent records;
    ExpirableRenovationComponent expirable;
#nullable enable

    /// <summary>For renovation access to prevent circular DI</summary>
    public BuildingRenovationService Service => service;

    string? pendingCurrentId;
    bool pendingMaterialsAcquired;
    Priority pendingPriority;

    public HashSet<string> ActiveRenovations { get; private set; } = [];

    /// <summary>In-progress job id, or null.</summary>
    public string? CurrentId { get; private set; }
    public RenovationSpec? CurrentSpec
        => CurrentId is null ? null
        : service.registry.TryGet(CurrentId, out var r) ? r.Spec : null;

    public Priority Priority { get; private set; } = Priority.Normal;

    public bool CanRenovate => CurrentId is null && blockObject.IsFinished;
    public bool IsRenovating => CurrentId is not null;
    public bool IsWaitingForMaterials => CurrentId is not null && distro.IsCollecting;
    public bool IsWorking => work.IsWorking;
    public bool MaterialsAcquired => CurrentId is not null && !distro.IsCollecting;
    public bool CanChangePriority => IsWaitingForMaterials;
    public bool IsFinished => blockObject.IsFinished;

    public RenovationDistroReceiver Distro => distro;
    public RenovationWorkComponent Work => work;
    public RenovationRecordComponent Records => records;
    public ExpirableRenovationComponent Expirable => expirable;

    public event Action<string>? RenovationStarted;
    public event Action<string>? RenovationFinished;
    public event Action<string>? RenovationCancelled;
    public event Action<string>? RenovationRemoved;
    public event Action<string>? RenovationExpired;

    public bool HasActive(string id) => ActiveRenovations.Contains(id);

    /// <summary>
    /// Soft unavailability for list/start: already-active, then the renovation's own reason.
    /// Does not check <see cref="RenovationBase.CanRenovate"/> — callers filter that separately.
    /// </summary>
    public string? GetUnavailableReason(RenovationBase renovation)
    {
        if (HasActive(renovation.Id))
        {
            return service.t.T("LV.BRe.AlreadyActive");
        }

        return renovation.GetUnavailableReason(this);
    }

    public void Awake()
    {
        blockObject = GetComponent<BlockObject>();
        distro = GetComponent<RenovationDistroReceiver>();
        work = GetComponent<RenovationWorkComponent>();
        records = GetComponent<RenovationRecordComponent>();
        expirable = GetComponent<ExpirableRenovationComponent>();

        lackOfResourcesStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "LackOfResources",
            service.t.T("LV.BRe.NoMaterialShort"),
            service.t.T("Status.ConstructionSites.NoMaterials"),
            1f);
        GetComponent<StatusSubject>().RegisterStatus(lackOfResourcesStatus);
    }

    public void StartRenovation(string renovationId, Priority priority)
    {
        if (!service.registry.TryGet(renovationId, out var renovation))
        {
            throw new InvalidOperationException($"Unknown renovation: {renovationId}");
        }

        if (!renovation.CanRenovate(this))
        {
            throw new InvalidOperationException($"Renovation {renovationId} is not applicable to this building");
        }

        var reason = GetUnavailableReason(renovation);
        if (reason is not null)
        {
            throw new InvalidOperationException(reason);
        }

        StartRenovation(renovation.Spec, priority, materialsAcquired: false, workProgress: 0f, fromSave: false);
    }

    void StartRenovation(RenovationSpec spec, Priority priority, bool materialsAcquired, float workProgress, bool fromSave)
    {
        if (!CanRenovate)
        {
            if (!blockObject.IsFinished)
            {
                Debug.LogWarning($"Cannot renovate unfinished building {this} with {spec.Id}");
                return;
            }

            throw new InvalidOperationException($"Already renovating: {CurrentId}, attempting: {spec.Id}");
        }

        CurrentId = spec.Id;
        Priority = priority;

        if (materialsAcquired)
        {
            BeginWork(spec, workProgress);
        }
        else if (fromSave)
        {
            distro.ResumeCollecting(priority);
            // ResumeCollecting may call OnMaterialsFullyStocked if already stocked.
            if (distro.IsCollecting)
            {
                lackOfResourcesStatus.Activate();
            }
        }
        else
        {
            distro.BeginCollecting(spec.Cost, priority);
            // BeginCollecting may call OnMaterialsFullyStocked if cost is empty/already stocked.
            if (distro.IsCollecting)
            {
                lackOfResourcesStatus.Activate();
            }
        }

        RenovationStarted?.Invoke(spec.Id);
    }

    /// <summary>Called by <see cref="RenovationDistroReceiver"/> when demand is fully met.</summary>
    public void OnMaterialsFullyStocked()
    {
        if (CurrentId is null || !distro.IsCollecting) { return; }
        if (CurrentSpec is not { } spec) { return; }

        BeginWork(spec, workProgress: 0f);
    }

    void BeginWork(RenovationSpec spec, float workProgress)
    {
        distro.MarkMaterialsCommitted();
        lackOfResourcesStatus.Deactivate();
        work.Start(spec.Id, spec.Days, workProgress);
    }

    /// <summary>Called by <see cref="RenovationWorkComponent"/> when the work timer completes.</summary>
    public void OnWorkCompleted(string renovationId)
    {
        if (CurrentId != renovationId) { return; }

        distro.ConsumeStoredGoods();
        records.Add(renovationId);
        ActiveRenovations.Add(renovationId);

        CurrentId = null;
        lackOfResourcesStatus.Deactivate();

        ApplyEffect(renovationId, isLoad: false);
        RenovationFinished?.Invoke(renovationId);
    }

    public void CancelCurrentRenovation()
    {
        if (CurrentId is null) { return; }

        var id = CurrentId;
        work.Cancel();
        distro.RefundAndClear();
        CurrentId = null;
        lackOfResourcesStatus.Deactivate();

        RenovationCancelled?.Invoke(id);
    }

    /// <summary>Remove an active renovation (permanent or timed) without expiring callbacks.</summary>
    public void DeactivateRenovation(string id)
    {
        if (!ActiveRenovations.Remove(id)) { return; }

        expirable.Cancel(id);
        RenovationRemoved?.Invoke(id);
    }

    /// <summary>Called by <see cref="ExpirableRenovationComponent"/> when a timed effect ends.</summary>
    public void OnRenovationExpired(string id)
    {
        ActiveRenovations.Remove(id);

        if (service.registry.TryGet(id, out var reno) && reno is ExpirableRenovationBase expirableReno)
        {
            expirableReno.OnExpired(this);
        }

        RenovationExpired?.Invoke(id);
    }

    public void ChangePriority(Priority priority)
    {
        if (!CanChangePriority)
        {
            throw new InvalidOperationException("Cannot change priority unless waiting for materials");
        }

        Priority = priority;
        distro.SetPriority(priority);
    }

    public void FinishNow()
    {
        if (CurrentId is null) { return; }

        if (distro.IsCollecting)
        {
            distro.MarkMaterialsCommitted();
            lackOfResourcesStatus.Deactivate();
            OnWorkCompleted(CurrentId);
            return;
        }

        if (work.IsWorking)
        {
            work.FinishNow();
        }
    }

    void ApplyEffect(string id, bool isLoad)
    {
        if (service.registry.TryGet(id, out var renovation))
        {
            renovation.OnCompleted(this, isLoad);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ActiveRenovationsKey, ActiveRenovations);

        if (CurrentId is not null)
        {
            s.Set(CurrentRenovationIdKey, CurrentId);
            s.Set(MaterialsAcquiredKey, !distro.IsCollecting);
            s.Set(PriorityKey, (int)Priority);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ActiveRenovationsKey))
        {
            ActiveRenovations = [.. s.Get(ActiveRenovationsKey)];
        }

        if (s.Has(CurrentRenovationIdKey))
        {
            pendingCurrentId = s.Get(CurrentRenovationIdKey);
            pendingMaterialsAcquired = s.Has(MaterialsAcquiredKey) && s.Get(MaterialsAcquiredKey);
            pendingPriority = s.Has(PriorityKey) ? (Priority)s.Get(PriorityKey) : Priority.Normal;
        }
    }

    public void PostLoadEntity()
    {
        foreach (var id in ActiveRenovations.ToArray())
        {
            ApplyEffect(id, isLoad: true);
        }

        if (pendingCurrentId is null) { return; }

        var idJob = pendingCurrentId;
        pendingCurrentId = null;

        if (!service.registry.TryGet(idJob, out var reno))
        {
            Debug.LogWarning($"Saved renovation {idJob} no longer exists; discarding.");
            distro.RefundAndClear();
            work.Cancel();
            return;
        }

        if (work.IsWorking)
        {
            CurrentId = idJob;
            Priority = pendingPriority;
            lackOfResourcesStatus.Deactivate();
            RenovationStarted?.Invoke(idJob);
            return;
        }

        if (pendingMaterialsAcquired)
        {
            CurrentId = idJob;
            Priority = pendingPriority;
            var progress = 0f;
            BeginWork(reno.Spec, progress);
            RenovationStarted?.Invoke(idJob);
            return;
        }

        StartRenovation(reno.Spec, pendingPriority, materialsAcquired: false, workProgress: 0f, fromSave: true);
    }

    public void OnEnterFinishedState() { }

    public void OnExitFinishedState()
    {
        expirable.ClearAll();
        work.Cancel();

        if (CurrentId is not null)
        {
            CancelCurrentRenovation();
        }
    }
}
