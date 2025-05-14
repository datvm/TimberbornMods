namespace BrainPowerSPs.Buffs;

public class WaterWheelUpBuffInst : CommonProjectBuffInstance<PowerBuffs>
{
    // We only create one buff effect using the best one
    public override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
    {
        var spec = info.Spec;
        var v1 = spec.Parameters[0];
        var v2 = spec.Parameters[1];

        return spec.Id == bestUpgrade?.Spec.Id
            ? ((v1 == v2)
                ? new PowerOutputMultiplierBuffEff(v1, t)
                : new PowerOutputDayMultiplierBuffEff(new Vector2(v1, v2), dayNight, t))
            : null;
    }

    ScientificProjectInfo? bestUpgrade;

    EntityManager entities = null!;
    IDayNightCycle dayNight = null!;
    [Inject]
    public void Inject(EntityManager entities, IDayNightCycle dayNight)
    {
        this.entities = entities;
        this.dayNight = dayNight;
    }

    public override void Init()
    {
        // Run this before base
        bestUpgrade = Value.OrderByDescending(q => q.Spec.Order).FirstOrDefault();

        base.Init();
    }

    public override IBuffTarget[] CreateTargets() => [new WaterWheelBuffTarget(ev, entities)];

    public override string GetBuffName(ILoc t) => t.T($"LV.BPSP.{bestUpgrade?.Spec.Id}");
    public override string GetBuffDescription(ILoc t) => t.T($"LV.BPSP.WaterWheelUpBuffDesc");

}

public class WaterWheelFlowUpBuffInst : CommonProjectBuffInstance<PowerBuffs>
{
    public override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateFlatOrLevelEffect(info, (value, name) => new GeneratorMinStrengthBuffEff(value, name, t));

    EntityManager entities = null!;
    [Inject]
    public void Inject(EntityManager entities)
    {
        this.entities = entities;
    }

    public override IBuffTarget[] CreateTargets() => [new WaterWheelBuffTarget(ev, entities)];
    public override string GetBuffName(ILoc t) => t.T("LV.BPSP.WaterWheelFlowUp1");
    public override string GetBuffDescription(ILoc t) => t.T("LV.BPSP.WaterWheelFlowUpBuffDesc");
}

public class WaterWheelBuffTarget(EventBus eventBus, EntityManager entities) : EntityBasedBuffTarget(eventBus)
{
    protected override bool Filter(EntityComponent entity) => !entity.GetComponentFast<WaterPoweredGenerator>();

    protected override HashSet<BuffableComponent> GetTargets() => [
        .. entities.Get<WaterPoweredGenerator>()
            .AsEnumerable()
            .Select(q => q.GetBuffable())];
}

