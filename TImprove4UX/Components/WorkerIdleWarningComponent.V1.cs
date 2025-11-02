namespace TImprove4UX.Components;

public class WorkerIdleWarningComponent : TickableComponent
{

#nullable disable
    WorkerIdleWarningService workerIdleWarningService;
    StatusToggle idleStatus;
    Enterable enterable;
#nullable enable

    public bool DisableWarning { get; private set; }

    public string TemplateName
    {
        get
        {
            var prefab = GetComponent<TemplateSpec>();
            return prefab.TemplateName;
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

        enterable = GetComponent<Enterable>();
        enterable.EntererAdded += Enterable_EntererAdded;
        enterable.EntererRemoved += Enterable_EntererRemoved;

        GetComponent<StatusSubject>().RegisterStatus(idleStatus);
        UpdateStatus(null);
    }

    void AddEnterer(Enterer e)
    {
        var bm = e.GetComponent<BehaviorManager>();
        if (!bm) { return; }
        insiders.Add(bm);
    }

    void Enterable_EntererRemoved(object sender, EntererRemovedEventArgs e)
    {
        var bm = e.Enterer.GetComponent<BehaviorManager>();
        if (bm is null) { return; } // Just check null here
        insiders.Remove(bm);
    }
    void Enterable_EntererAdded(object sender, EntererAddedEventArgs e) => AddEnterer(e.Enterer);

    public void ToggleDisableWarning(bool disabled)
    {
        if (disabled == DisableWarning) { return; }

        workerIdleWarningService.ToggleWarningDisabled(TemplateName, disabled);
    }

    public void UpdateStatus(bool? status)
    {
        status ??= workerIdleWarningService.IsWarningDisabled(TemplateName);

        if (status.Value == DisableWarning) { return; }
        DisableWarning = status.Value;

        if (DisableWarning)
        {
            DisableComponent();
            DisableStatus();            
        }
        else
        {
            EnableComponent();
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
