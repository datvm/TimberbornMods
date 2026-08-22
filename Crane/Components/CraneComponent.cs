namespace Crane.Components;

[AddTemplateModule2(typeof(CraneSpec))]
public class CraneComponent(
    CraneStructureService craneStructureService,
    CraneClaimService craneClaimService,
    ConstructionModeService constructionModeService,
    EntitySelectionService entitySelectionService,
    RangeTileMarkerService rangeTileMarkerService
) : BaseComponent, IAwakableComponent, IFinishedStateListener, IDeletableEntity, ISelectionListener, IBuildingWithRange
{

    BlockObject bo = null!;
    Workplace workplace = null!;
    PausableBuilding? pausable;
    Demolishable? demolishable;
    readonly List<ICraneRangeModifier> rangeModifiers = [];
    readonly List<ICraneRubbleProcessor> rubbleProcessors = [];
    int horizontalRange = CraneStructureService.TowerRange;

    public Vector3Int Coordinates => bo.Coordinates;
    public bool IsFinished => bo.IsFinished;

    public bool IsWorking => IsFinished && workplace.AssignedWorkers.Count > 0;
    public event EventHandler? WorkingStateChanged;
    bool prevState;

    public CraneTower Tower { get; internal set; } = null!;
    public int HorizontalRange => horizontalRange;

    public string RangeName => "Crane";

    public void Awake()
    {
        bo = GetComponent<BlockObject>();
        workplace = GetComponent<Workplace>();
        pausable = GetComponent<PausableBuilding>();
        demolishable = GetComponent<Demolishable>();
        Tower = new(this);

        workplace.WorkerAssigned += OnWorkerChanged;
        workplace.WorkerUnassigned += OnWorkerChanged;

        GetComponents(rangeModifiers);
        foreach (var modifier in rangeModifiers)
        {
            modifier.OnRangeChanged += OnRangeModifierChanged;
        }

        RecalculateRange();
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
        DetachRangeModifiers();
        rubbleProcessors.Clear();
        Tower.ClearJobs();
    }

    public void AddRangeModifier(ICraneRangeModifier modifier)
    {
        if (rangeModifiers.Contains(modifier))
        {
            return;
        }

        rangeModifiers.Add(modifier);
        modifier.OnRangeChanged += OnRangeModifierChanged;
        RecalculateRange();
    }

    public void RemoveRangeModifier(ICraneRangeModifier modifier)
    {
        if (!rangeModifiers.Remove(modifier))
        {
            return;
        }

        modifier.OnRangeChanged -= OnRangeModifierChanged;
        RecalculateRange();
    }

    public void AddRubbleProcessor(ICraneRubbleProcessor processor)
    {
        if (rubbleProcessors.Contains(processor))
        {
            return;
        }

        rubbleProcessors.Add(processor);
    }

    public void RemoveRubbleProcessor(ICraneRubbleProcessor processor) => rubbleProcessors.Remove(processor);

    public bool TryProcessRubble(RecoveredGoodStack stack, int items)
    {
        foreach (var processor in rubbleProcessors)
        {
            if (processor.TryProcessRubble(this, stack, items))
            {
                return true;
            }
        }

        return false;
    }

    void OnRangeModifierChanged(object sender, EventArgs e) => RecalculateRange();

    void RecalculateRange()
    {
        var range = CraneStructureService.TowerRange;
        foreach (var modifier in rangeModifiers)
        {
            range += modifier.RangeDelta;
        }

        range = Math.Max(1, range);
        if (range == horizontalRange)
        {
            return;
        }

        horizontalRange = range;
        craneStructureService.NotifyRangeChanged(Tower);
        RefreshRangeIndicator();
    }

    void RefreshRangeIndicator()
    {
        if (!entitySelectionService.IsAnythingSelected)
        {
            return;
        }

        var selected = entitySelectionService.SelectedObject.GetComponent<IBuildingWithRange>();
        if (selected is null || selected.RangeName != RangeName)
        {
            return;
        }

        rangeTileMarkerService.RecalculateArea(RangeName);
    }

    void DetachRangeModifiers()
    {
        foreach (var modifier in rangeModifiers)
        {
            modifier.OnRangeChanged -= OnRangeModifierChanged;
        }

        rangeModifiers.Clear();
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

    public void OnSelect() => constructionModeService.EnterConstructionMode();
    public void OnUnselect() { }

    public IEnumerable<Vector3Int> GetBlocksInRange()
    {
        var origin = Coordinates;
        var range = HorizontalRange;
        for (var x = origin.x - range; x <= origin.x + range; x++)
        {
            for (var y = origin.y - range; y <= origin.y + range; y++)
            {
                yield return new(x, y, origin.z);
            }
        }
    }

    public IEnumerable<BaseComponent> GetObjectsInRange() => [];
}
