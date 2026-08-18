namespace Crane.Components;

[AddTemplateModule2(typeof(CraneSpec))]
public class CraneComponent(
    CraneStructureService craneStructureService,
    CraneClaimService craneClaimService
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IDeletableEntity
{

    BlockObject bo = null!;
    Workplace workplace = null!;
    PausableBuilding? pausable;
    Demolishable? demolishable;

    public Vector3Int Coordinates => bo.Coordinates;
    public bool IsFinished => bo.IsFinished;

    public bool IsWorking => IsFinished && workplace.AssignedWorkers.Count > 0;
    public event EventHandler? WorkingStateChanged;
    bool prevState;

    public CraneTower Tower { get; internal set; } = null!;

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        workplace = GetComponent<Workplace>();
        pausable = GetComponent<PausableBuilding>();
        demolishable = GetComponent<Demolishable>();
        Tower = new(this);

        workplace.WorkerAssigned += OnWorkerChanged;
        workplace.WorkerUnassigned += OnWorkerChanged;
    }

    void OnWorkerChanged(object sender, WorkerChangedEventArgs e) => CheckAndRaiseStateChange();

    public void OnEnterFinishedState()
    {
        if (pausable)
        {
            pausable!.PausedChanged += OnEligibilityChanged;
        }

        if (demolishable)
        {
            demolishable!.Marked += OnEligibilityChanged;
            demolishable.Unmarked += OnEligibilityChanged;
        }

        craneStructureService.RefreshCraneStructure(this);
        CheckAndRaiseStateChange();
    }

    public void OnExitFinishedState()
    {
        DetachEligibility();
        craneStructureService.RefreshCraneStructure(this);
        craneClaimService.Refresh(this);
        CheckAndRaiseStateChange();
    }

    public void DeleteEntity()
    {
        DetachEligibility();
        Tower.ClearJobs();
    }

    void OnEligibilityChanged(object sender, EventArgs e) => craneClaimService.Refresh(this);

    void DetachEligibility()
    {
        if (pausable)
        {
            pausable!.PausedChanged -= OnEligibilityChanged;
        }

        if (demolishable)
        {
            demolishable!.Marked -= OnEligibilityChanged;
            demolishable.Unmarked -= OnEligibilityChanged;
        }
    }

    void CheckAndRaiseStateChange()
    {
        var curr = IsWorking;
        if (curr != prevState)
        {
            prevState = curr;
            WorkingStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
