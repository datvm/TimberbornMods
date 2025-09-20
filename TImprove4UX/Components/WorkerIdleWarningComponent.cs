namespace TImprove4UX.Components;

public class WorkerIdleWarningComponent : TickableComponent
{

#nullable disable
    WorkerIdleWarningService workerIdleWarningService;
    StatusToggle idleStatus;
    Enterable enterable;
#nullable enable

    public bool DisableWarning { get; private set; }

    public string PrefabName
    {
        get
        {
            var prefab = GetComponentFast<PrefabSpec>();
            return prefab.PrefabName;
        }
    }

    readonly HashSet<BehaviorManager> insiders = [];

    [Inject]
    public void Inject(WorkerIdleWarningService workerIdleWarningService, ILoc t)
    {
        this.workerIdleWarningService = workerIdleWarningService;

        idleStatus = StatusToggle.CreateNormalStatusWithAlertAndFloatingIcon(
            "NothingToDo",
            t.T("LV.T4UX.WorkerIdleStatus"),
            t.T("LV.T4UX.WorkerIdleStatusShort"),
            1
        );
    }

    public override void StartTickable()
    {
        base.StartTickable();

        enterable = GetComponentFast<Enterable>();
        enterable.EntererAdded += Enterable_EntererAdded;
        enterable.EntererRemoved += Enterable_EntererRemoved;

        GetComponentFast<StatusSubject>().RegisterStatus(idleStatus);
        UpdateStatus(null);
    }

    void AddEnterer(Enterer e)
    {
        var bm = e.GetComponentFast<BehaviorManager>();
        if (!bm) { return; }
        insiders.Add(bm);
    }

    private void Enterable_EntererRemoved(object sender, EntererRemovedEventArgs e)
    {
        var bm = e.Enterer.GetComponentFast<BehaviorManager>();
        if (bm is null) { return; } // Just check null here
        insiders.Remove(bm);
    }
    private void Enterable_EntererAdded(object sender, EntererAddedEventArgs e) => AddEnterer(e.Enterer);

    public void ToggleDisableWarning(bool disabled)
    {
        if (disabled == DisableWarning) { return; }

        workerIdleWarningService.ToggleWarningDisabled(PrefabName, disabled);
    }

    public void UpdateStatus(bool? status)
    {
        status ??= workerIdleWarningService.IsWarningDisabled(PrefabName);

        if (status.Value == DisableWarning) { return; }
        DisableWarning = status.Value;

        if (DisableWarning)
        {
            enabled = false;
            DisableStatus();            
        }
        else
        {
            enabled = true;
            CheckForIdleWorkers();
        }
    }

    public void CheckForIdleWorkers()
    {
        var hasIdleWorker = false;

        foreach (var e in insiders)
        {
            if (e.IsRunningBehavior<WaitInsideIdlyWorkplaceBehavior>())
            {
                hasIdleWorker = true;
                break;
            }
        }

        if (hasIdleWorker)
        {
            EnableStatus();
        }
        else
        {
            DisableStatus();
        }
    }

    public void EnableStatus()
    {
        idleStatus.Activate();
    }

    public void DisableStatus()
    {
        idleStatus.Deactivate();
    }

    public override void Tick() => CheckForIdleWorkers();
}
