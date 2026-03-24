namespace BathBombs.Components;

public readonly record struct BathBombWater(float Amount, bool Contaminated);

public class BathBombComponent(BathBombService service) : TickableComponent, IAwakableComponent, IPersistentEntity, IEntityDescriber
{
    static readonly ComponentKey SaveKey = new(nameof(BathBombComponent));
    static readonly PropertyKey<int> TicksToDetonationKey = new("TicksToDetonation");

#nullable disable
    public BathBombSpec spec;
#nullable enable

    public BathBombWater Water => new(spec.Amount, spec.Contaminated);
    public Vector3Int Coordinates => GetComponent<BlockObject>().Coordinates;
    public string ExplosionPrefabPath => spec.ExplosionPrefabPath;

    int ticksToDetonation = -1;
    public bool Triggered => Enabled && ticksToDetonation >= 0;

    public void Awake()
    {
        spec = GetComponent<BathBombSpec>();

        if (ticksToDetonation < 0)
        {
            DisableComponent();
        }
    }

    public void Trigger()
    {
        ticksToDetonation = spec.DetonationTicks;
        EnableComponent();
    }

    void Detonate()
    {
        service.Detonate(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(TicksToDetonationKey))
        {
            ticksToDetonation = s.Get(TicksToDetonationKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (ticksToDetonation < 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(TicksToDetonationKey, ticksToDetonation);
    }

    public override void Tick()
    {
        ticksToDetonation--;

        if (ticksToDetonation <= 0)
        {
            DisableComponent();
            Detonate();
        }
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [
        EntityDescription.CreateTextSection(service.GetDescription(spec.Amount), 100),
    ];
}
