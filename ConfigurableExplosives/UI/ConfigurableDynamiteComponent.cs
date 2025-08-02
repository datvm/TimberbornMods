namespace ConfigurableExplosives.UI;

public class ConfigurableDynamiteComponent : TickableComponent, IPersistentEntity, IInitializableEntity
{
    static readonly ComponentKey SaveKey = new("ConfigurableDynamiteComponent");
    static readonly PropertyKey<float> DelayKey = new("Delay");
    static readonly PropertyKey<int> DepthKey = new("Depth");
    static readonly PropertyKey<int> TriggerRadiusKey = new("TriggerRadius");

    public float DetonationDelay { get; set; }
    public int DetonationDepth
    {
        get => field == 0 ? MaxDepth : field;
        set => field = Math.Min(MaxDepth, value);
    }
    public int TriggerRadius { get; set; } = 1;

    public bool Triggered => dynamite.Triggered;
    public float TriggeredTime { get; private set; }

    public bool ShouldDetonate => Triggered && TriggeredTime >= DetonationDelay;

    public int MaxDepth => dynamite._dynamiteSpec.Depth; // Use the spec, not the property because it will be patched
    Dynamite dynamite = null!;
    ConfigurableDynamiteFragment fragment = null!;

    public ConfigurableDynamiteComponentData Data => new(DetonationDelay, DetonationDepth, TriggerRadius);

    [Inject]
    public void Inject(ConfigurableDynamiteFragment fragment)
    {
        this.fragment = fragment;
    }

    public void Awake()
    {
        dynamite = GetComponentFast<Dynamite>();
    }

    public override void StartTickable()
    {
        if (DetonationDepth == 0)
        {
            DetonationDepth = MaxDepth;
        }
    }

    public override void Tick()
    {
        if (!Triggered) { return; }

        TriggeredTime += Time.fixedDeltaTime;
        if (ShouldDetonate)
        {
            dynamite.Detonate();
        }
    }

    public void CopyFrom(ConfigurableDynamiteComponent other) => CopyFrom(other.Data);

    public void CopyFrom(ConfigurableDynamiteComponentData data)
    {
        DetonationDelay = data.DetonationDelay;
        DetonationDepth = data.DetonationDepth;
        TriggerRadius = data.TriggerRadius;
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        s.Set(DelayKey, DetonationDelay);
        s.Set(DepthKey, DetonationDepth);
        s.Set(TriggerRadiusKey, TriggerRadius);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(DelayKey))
        {
            DetonationDelay = s.Get(DelayKey);
        }

        if (s.Has(DepthKey))
        {
            DetonationDepth = s.Get(DepthKey);
        }

        if (s.Has(TriggerRadiusKey))
        {
            TriggerRadius = s.Get(TriggerRadiusKey);
        }
    }

    public void InitializeEntity()
    {
        if (fragment.Template is null) { return; }

        CopyFrom(fragment.Template.Value);
    }
}

public readonly record struct ConfigurableDynamiteComponentData(float DetonationDelay, int DetonationDepth, int TriggerRadius);