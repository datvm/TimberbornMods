namespace UnstableCoreChallenge.Components;

public class UnstableCoreStabilizer(UnstableCoreStabilizerService service) : BaseComponent, IPersistentEntity, IAwakableComponent, IInitializableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(UnstableCoreStabilizer));
    static readonly PropertyKey<int> DaysKey = new("Days");
    static readonly PropertyKey<string> GoodsCostKey = new("GoodsCost");
    static readonly PropertyKey<string> GoodsPaidKey = new("GoodsPaid");

    UnstableCoreStabilizerStat stat;
    Dictionary<string, int> goodsPaid = [];

#nullable disable
    TimedComponentActivator timedComponentActivator;
#nullable enable

    public void Awake()
    {
        timedComponentActivator = GetComponent<TimedComponentActivator>();
    }

    public void InitializeEntity()
    {
        if (stat != default) { return; }

        if (GetComponent<UnstableCoreStabilizerInitializer>() is { } initializer)
        {
            InitializeNewChallenge(initializer.Stat);
        }
        else // Not from  the challenge
        {
            StabilizeAndDestroy();
        }
    }

    void InitializeNewChallenge(UnstableCoreStabilizerStat stat)
    {
        this.stat = stat;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        var days = s.Get(DaysKey);

    }

    public void Save(IEntitySaver entitySaver)
    {

    }

    public void Stabilize()
    {
        GetComponent<UnstableCoreExplosionBlocker>().BlockExplosion();
    }

    public void StabilizeAndDestroy()
    {
        Stabilize();
        service.Delete(this);
    }

}
