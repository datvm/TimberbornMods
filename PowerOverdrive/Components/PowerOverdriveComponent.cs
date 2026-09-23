namespace PowerOverdrive.Components;

[AddTemplateModule2(typeof(Manufactory))]
public class PowerOverdriveComponent(PowerOverdriveService service)
    : BaseComponent,
        IAwakableComponent,
        IInitializableEntity,
        IPersistentEntity,
        IFinishedStateListener,
        IDeletableEntity,
        IDuplicable<PowerOverdriveComponent>,
        IModdableMechanicalNodeModifier
{
    static readonly ComponentKey SaveKey = new(nameof(PowerOverdriveComponent));
    static readonly PropertyKey<int> LevelKey = new("Level");

    bool finished;
    float powerMultiplier = 1f;

    public PowerOverdriveService Service => service;
    public bool Eligible { get; private set; }
    public int ActiveLevel { get; private set; }
    public float ProductionMultiplier { get; private set; } = 1f;

    public string Id { get; } = nameof(PowerOverdriveComponent);
    public int Priority { get; } = (int)ModifierPriority.Multiplicative;
    public bool Disabled { get; private set; } = true;
    public event Action? OnChanged;

    public void Awake()
    {
        var node = GetComponent<MechanicalNodeSpec>();
        Eligible = node is not null && node.PowerInput > 0;
    }

    public void InitializeEntity() => RefreshEffects();

    public void OnEnterFinishedState()
    {
        finished = true;
        RefreshEffects();
    }

    public void OnExitFinishedState()
    {
        finished = false;
        RefreshEffects();
    }

    public void DeleteEntity() => service.Deactivate(this);

    public void SetActiveLevel(int level)
    {
        ActiveLevel = level;
        RefreshEffects();
    }

    public bool TrySetActiveLevel(int level, out string? error)
        => service.TrySetActiveLevel(this, level, out error);

    public void Save(IEntitySaver entitySaver)
    {
        if (ActiveLevel == 0)
        {
            return;
        }

        entitySaver.GetComponent(SaveKey).Set(LevelKey, ActiveLevel);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s) || !s.Has(LevelKey))
        {
            return;
        }

        var level = s.Get(LevelKey);
        if (level < 1 || level > service.LevelCount)
        {
            return;
        }

        ActiveLevel = level;
    }

    public void DuplicateFrom(PowerOverdriveComponent source)
    {
        if (source.ActiveLevel == ActiveLevel)
        {
            return;
        }

        if (source.ActiveLevel == 0)
        {
            service.Deactivate(this);
            return;
        }

        service.TrySetActiveLevel(this, source.ActiveLevel, out _);
    }

    public bool Modify(ModdableMechanicalNodeValues value)
    {
        if (value.Value.NominalInput == 0)
        {
            return false;
        }

        value.Value = value.Value with
        {
            NominalInput = Mathf.CeilToInt(value.Value.NominalInput * powerMultiplier)
        };
        return false;
    }

    void RefreshEffects()
    {
        var active = Eligible && finished && ActiveLevel > 0;
        var production = active ? service.BonusAt(ActiveLevel).ProductionBonus : 0f;
        var power = active ? service.BonusAt(ActiveLevel).PowerUsageBonus : 0f;
        var nextProduction = 1f + production;
        var nextPower = 1f + power;
        var nextDisabled = power == 0f;

        ProductionMultiplier = nextProduction;

        if (Disabled != nextDisabled || !Mathf.Approximately(powerMultiplier, nextPower))
        {
            Disabled = nextDisabled;
            powerMultiplier = nextPower;
            OnChanged?.Invoke();
        }
    }
}
