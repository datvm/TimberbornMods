namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchet(
    ILoc t,
    IGoodService goods,
    TrebuchetTrajectoryService trajectory
) : BaseComponent, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(CraneHeadTrebuchet));
    static readonly PropertyKey<int> ModeKey = new("Mode");

    CraneHeadTrebuchetSpec spec = null!;
    CraneHeadComponent head = null!;
    BlockObject bo = null!;

    public CraneHeadTrebuchetSpec Spec => spec;
    public CraneHeadComponent Head => head;
    public bool IsFinished => bo.IsFinished;
    public TrebuchetLaunchMode Mode { get; private set; } = TrebuchetLaunchMode.None;
    public Vector3Int Origin => bo.Coordinates;
    public int MaxRange => head.Crane?.Tower.Sections.Count ?? 0;
    public int PeakDelta => MaxRange;

    public event EventHandler? ModeChanged;

    public void Awake()
    {
        spec = GetComponent<CraneHeadTrebuchetSpec>();
        head = GetComponent<CraneHeadComponent>();
        bo = GetComponent<BlockObject>();
    }

    public void Save(IEntitySaver entitySaver)
        => entitySaver.GetComponent(SaveKey).Set(ModeKey, (int)Mode);

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s) || !s.Has(ModeKey))
        {
            return;
        }

        Mode = (TrebuchetLaunchMode)s.Get(ModeKey);
    }

    public void SetMode(TrebuchetLaunchMode mode)
    {
        if (Mode == mode)
        {
            return;
        }

        Mode = mode;
        ModeChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool InRange(Vector3Int dest) => trajectory.InRange(Origin, dest, MaxRange);

    public string CostDescription()
    {
        List<string> parts = [];
        foreach (var cost in spec.LaunchCost)
        {
            if (string.IsNullOrEmpty(cost.Id) || cost.Amount <= 0)
            {
                continue;
            }

            parts.Add($"{cost.Amount} {goods.GetGood(cost.Id).DisplayName.Value}");
        }

        return parts.Count == 0 ? t.T("LV.CrH.NoCost") : string.Join(", ", parts);
    }
}
