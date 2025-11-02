namespace UnstableCoreChallenge.Components;

public class UnstableCoreStabilizer : BaseComponent, IPersistentEntity, IPostInitializableEntity, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(UnstableCoreStabilizer));
    static readonly PropertyKey<int> DaysKey = new("Days");
    static readonly PropertyKey<int> ScienceCostKey = new("ScienceCost");
    static readonly PropertyKey<int> SciencePaidKey = new("SciencePaid");
    static readonly ListKey<string> GoodsCostKey = new("GoodsCost");
    static readonly ListKey<string> GoodsPaidKey = new("GoodsPaid");

#nullable disable
    UnstableCoreService service;
    TimedComponentActivator timedComponentActivator;
#nullable enable
    StabilizerRecord? record;

    int? sciencePaid;
    Dictionary<string, int> goodsPaid = [];

    [Inject]
    public void InjectDependencies(UnstableCoreService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        timedComponentActivator = GetComponent<TimedComponentActivator>();
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        var days = s.Get(DaysKey);
        int? scienceCost = s.Has(ScienceCostKey) ? s.Get(ScienceCostKey) : null;
        var goodsCost = s.Get(GoodsCostKey)
            .Select(q =>
            {
                var parts = q.Split(';');
                return new GoodAmount(parts[0], int.Parse(parts[1]));
            })
            .ToImmutableArray();
        record = new(days, scienceCost, goodsCost);

        if (s.Has(SciencePaidKey))
        {
            sciencePaid = s.Get(SciencePaidKey);
        }
        goodsPaid = s.Get(GoodsPaidKey)
            .Select(q =>
            {
                var parts = q.Split(';');
                return new KeyValuePair<string, int>(parts[0], int.Parse(parts[1]));
            })
            .ToDictionary(q => q.Key, q => q.Value);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (record is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(DaysKey, record.Days);
        if (record.Science.HasValue)
        {
            s.Set(ScienceCostKey, record.Science.Value);
        }
        s.Set(GoodsCostKey, [.. record.Goods.Select(q => q.GoodId + ";" + q.Amount)]);

        if (sciencePaid.HasValue)
        {
            s.Set(SciencePaidKey, sciencePaid.Value);
        }
        s.Set(GoodsPaidKey, [.. goodsPaid.Select(q => q.Key + ";" + q.Value)]);
    }

    public void Stablize()
    {
        GetComponent<UnstableCoreExplosionBlocker>().BlockExplosion();
    }

    public void PostInitializeEntity()
    {
        record ??= service.InitializeNewCore();
        if (!timedComponentActivator.CountdownIsActive)
        {
            timedComponentActivator.CyclesUntilCountdownActivation = 0;
            timedComponentActivator.DaysUntilActivation = record.Days;
            timedComponentActivator.OnCycleDayStarted(null);
        }
    }
}
