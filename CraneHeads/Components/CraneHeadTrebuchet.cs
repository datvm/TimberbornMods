namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchet(
    ILoc t,
    IGoodService goods,
    IDayNightCycle clock,
    TrebuchetTrajectoryService trajectory
) : BaseComponent, IAwakableComponent, IPersistentEntity, IFinishedPausable, IInitializableEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(CraneHeadTrebuchet));
    static readonly PropertyKey<int> ModeKey = new("Mode");
    static readonly PropertyKey<float> LaunchedAtKey = new("LaunchedAt");

    CraneHeadTrebuchetSpec spec = null!;
    CraneHeadComponent head = null!;
    BlockObject bo = null!;
    BlockableObject blockable = null!;
    float launchedAt;

    public CraneHeadTrebuchetSpec Spec => spec;
    public CraneHeadComponent Head => head;
    public bool IsFinished => bo.IsFinished;
    public bool IsPaused => blockable && !blockable.IsUnblocked;
    public TrebuchetLaunchMode Mode { get; private set; } = TrebuchetLaunchMode.None;
    public Vector3Int Origin => bo.Coordinates;
    public int MaxRange => head.Crane?.Tower.Sections.Count ?? 0;
    public int PeakDelta => MaxRange;
    public bool IsOnCooldown => RemainingCooldownHours > 0f;
    public float RemainingCooldownHours
    {
        get
        {
            if (spec.CooldownHours <= 0)
            {
                return 0f;
            }

            var elapsed = (clock.PartialDayNumber - launchedAt) * 24f;
            return Math.Max(0f, spec.CooldownHours - elapsed);
        }
    }

    public event EventHandler? ModeChanged;
    public event EventHandler? PausedChanged;

    public void Awake()
    {
        spec = GetComponent<CraneHeadTrebuchetSpec>();
        head = GetComponent<CraneHeadComponent>();
        bo = GetComponent<BlockObject>();
        blockable = GetComponent<BlockableObject>();
    }

    public void InitializeEntity()
    {
        if (!blockable)
        {
            return;
        }

        blockable.ObjectBlocked += OnBlockedChanged;
        blockable.ObjectUnblocked += OnBlockedChanged;
    }

    public void DeleteEntity()
    {
        if (!blockable)
        {
            return;
        }

        blockable.ObjectBlocked -= OnBlockedChanged;
        blockable.ObjectUnblocked -= OnBlockedChanged;
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ModeKey, (int)Mode);
        s.Set(LaunchedAtKey, launchedAt);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(ModeKey))
        {
            Mode = (TrebuchetLaunchMode)s.Get(ModeKey);
        }

        if (s.Has(LaunchedAtKey))
        {
            launchedAt = s.Get(LaunchedAtKey);
        }
    }

    public void MarkLaunched()
    {
        launchedAt = clock.PartialDayNumber;
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

    void OnBlockedChanged(object sender, EventArgs e)
    {
        PausedChanged?.Invoke(this, EventArgs.Empty);
    }
}
