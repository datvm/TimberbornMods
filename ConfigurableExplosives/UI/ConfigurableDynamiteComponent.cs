global using Timberborn.TickSystem;

namespace ConfigurableExplosives.UI;

public class ConfigurableDynamiteComponent : TickableComponent, IPersistentEntity
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

    public void CopyFrom(ConfigurableDynamiteComponent other)
    {
        DetonationDelay = other.DetonationDelay;
        DetonationDepth = other.DetonationDepth;
        TriggerRadius = other.TriggerRadius;
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

}
