namespace BrainPowerSPs.Buffs;

public class WindmillBuffInstant : CommonProjectBuffInstance<PowerBuffs>
{

    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateFlatEffect(info, (v, _) => new WindmillHeightBuffEff(v, t));

    protected override IBuffTarget[] CreateTargets() => [new WindmillBuffTarget(ev, entities)];

    EntityManager entities = null!;
    [Inject]
    public void Inject(EntityManager entities)
    {
        this.entities = entities;
    }

    protected override string GetBuffName(ILoc t) => t.T("LV.BPSP.WindmillHeightUp");
    protected override string GetBuffDescription(ILoc t) => t.T("LV.BPSP.WindmillHeightUpLore");

}

public class WindmillBuffTarget(EventBus eventBus, EntityManager entities) : EntityBasedBuffTarget(eventBus)
{
    protected override bool Filter(EntityComponent entity)
        => entity.GetComponentFast<WindPoweredGeneratorSpec>();

    protected override HashSet<BuffableComponent> GetTargets()
        => [.. entities.Get<WindPoweredGeneratorSpec>()
            .AsEnumerable()
            .Select(q => q.GetBuffable())];
}