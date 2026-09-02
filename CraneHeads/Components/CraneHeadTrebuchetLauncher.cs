namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchetLauncher(
    RecoveredGoodStackSpawner spawner,
    TrebuchetTrajectoryService trajectory,
    TrebuchetShotEffectService effects
) : TickableComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(CraneHeadTrebuchetLauncher));
    static readonly PropertyKey<Vector3Int> TargetKey = new("Target");

    CraneHeadTrebuchet trebuchet = null!;
    CraneHeadTrebuchetInventory inventory = null!;
    BlockObject bo = null!;
    readonly List<GoodAmount> payload = [];

    public Vector3Int? Target { get; private set; }
    public bool IsFlying => effects.IsFlying(this);
    public bool CanFire
        => trebuchet.IsFinished
            && !trebuchet.IsPaused
            && trebuchet.Mode != TrebuchetLaunchMode.Repeat
            && inventory.IsReady
            && !IsFlying
            && !trebuchet.IsOnCooldown;

    public event EventHandler? TargetChanged;
    public event EventHandler? TrajectoryChecked;

    public void Awake()
    {
        trebuchet = GetComponent<CraneHeadTrebuchet>();
        inventory = GetComponent<CraneHeadTrebuchetInventory>();
        bo = GetComponent<BlockObject>();
        DisableComponent();
    }

    public void InitializeEntity()
    {
        inventory.ReadyChanged += OnReadyChanged;
        trebuchet.ModeChanged += OnModeChanged;
        trebuchet.PausedChanged += OnPausedChanged;
        if (trebuchet.IsOnCooldown)
        {
            EnableComponent();
        }
    }

    public override void Tick()
    {
        if (trebuchet.IsOnCooldown)
        {
            return;
        }

        DisableComponent();
        TryLaunch();
    }

    public void DeleteEntity()
    {
        inventory.ReadyChanged -= OnReadyChanged;
        trebuchet.ModeChanged -= OnModeChanged;
        trebuchet.PausedChanged -= OnPausedChanged;
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (Target is { } target)
        {
            entitySaver.GetComponent(SaveKey).Set(TargetKey, target);
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s) || !s.Has(TargetKey))
        {
            return;
        }

        Target = s.Get(TargetKey);
    }

    public TrebuchetShotCheck Evaluate(Vector3Int dest)
        => trajectory.Evaluate(trebuchet.Origin, dest, trebuchet.MaxRange, trebuchet.PeakDelta, bo);

    public bool CanHit(Vector3Int dest) => Evaluate(dest).IsValid;

    public bool IsTrajectoryValid() => Target is { } dest && CanHit(dest);

    public void SetTarget(Vector3Int dest)
    {
        Target = dest;
        TargetChanged?.Invoke(this, EventArgs.Empty);
        TryLaunch();
    }

    public void OnShown() => TryLaunch();

    public void Fire()
    {
        if (!CanFire)
        {
            return;
        }

        trebuchet.SetMode(TrebuchetLaunchMode.Once);
    }

    public void SetRepeat(bool repeat)
    {
        trebuchet.SetMode(repeat ? TrebuchetLaunchMode.Repeat : TrebuchetLaunchMode.None);
    }

    public void FinishShot()
    {
        if (!this || !trebuchet.IsFinished || trebuchet.Mode == TrebuchetLaunchMode.None || !inventory.IsReady)
        {
            return;
        }

        if (Target is not { } dest || !IsTrajectoryValid())
        {
            TrajectoryChecked?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (!inventory.TryRemoveForLaunch(payload))
        {
            return;
        }

        spawner.AddAwaitingGoods(dest, payload);
        if (trebuchet.Mode == TrebuchetLaunchMode.Once)
        {
            trebuchet.SetMode(TrebuchetLaunchMode.None);
        }

        TryLaunch();
    }

    void OnReadyChanged(object sender, EventArgs e)
    {
        if (inventory.IsReady)
        {
            TryLaunch();
        }
    }

    void OnModeChanged(object sender, EventArgs e) => TryLaunch();

    void OnPausedChanged(object sender, EventArgs e)
    {
        if (!trebuchet.IsPaused)
        {
            TryLaunch();
        }
    }

    void TryLaunch()
    {
        if (!trebuchet.IsFinished || trebuchet.IsPaused || trebuchet.Mode == TrebuchetLaunchMode.None)
        {
            return;
        }

        if (Target is null || !IsTrajectoryValid())
        {
            TrajectoryChecked?.Invoke(this, EventArgs.Empty);
            return;
        }

        TrajectoryChecked?.Invoke(this, EventArgs.Empty);
        if (!inventory.IsReady || trebuchet.IsOnCooldown)
        {
            return;
        }

        if (!effects.TryStart(this))
        {
            return;
        }

        trebuchet.MarkLaunched();
        if (trebuchet.IsOnCooldown)
        {
            EnableComponent();
        }
    }
}
