namespace WeatherScientificProjects.Processors;

public class WeatherBuff(ISingletonLoader loader, IBuffService buffs, EventBus eb, ScientificProjectService projects) : SimpleBuff(loader, buffs)
{
    readonly IBuffService buffs = buffs;

    static readonly SingletonKey SaveKey = new("WeatherBuff");

    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = "";
    public override string Description { get; protected set; } = "";

    protected override void AfterLoad()
    {
        base.AfterLoad();
        RefreshBuffs();

        eb.Register(this);
    }

    [OnEvent]
    public void OnProjectPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        // For these projects, the unlock project does nothing so we only care about this
        RefreshBuffs();
    }

    void RefreshBuffs()
    {
        buffs.RemoveAllInstances<WeatherWaterSourceBuffInstance>();

        foreach (var id in WeatherProjectsUtils.FreshWaterStrengthIds)
        {
            RefreshBuff(id, true);
        }

        foreach (var id in WeatherProjectsUtils.BadWaterStrengthIds)
        {
            RefreshBuff(id, false);
        }
    }

    void RefreshBuff(string id, bool fresh)
    {
        var p = projects.GetProject(id);
        if (p.TodayLevel == 0) { return; }

        var instance = buffs.CreateBuffInstance<WeatherBuff, WeatherWaterSourceBuffInstance, WaterSourceBuffInstanceValue>(
            this,
            new(fresh, p.TodayLevel * p.Spec.Parameters[0]));
        buffs.Apply(instance);
    }

}

public readonly record struct WaterSourceBuffInstanceValue(bool Fresh, float Modifier);

public class WeatherWaterSourceBuffInstance : BuffInstance<WaterSourceBuffInstanceValue, WeatherBuff>
{

    public override bool IsBuff { get; protected set; } = true;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];
    public WeatherWaterSourceBuffEffect Effect { get; private set; } = null!;

    EntityManager entities = null!;
    EventBus eb = null!;
    ILoc t = null!;

    [Inject]
    public void Inject(EntityManager entities, EventBus eb, ILoc t)
    {
        this.entities = entities;
        this.eb = eb;
        this.t = t;
    }

    public override void Init()
    {
        base.Init();

        IsBuff = Value.Modifier > 0;
        Targets = [new WeatherWaterSourceBuffTarget(entities, Value.Fresh, eb)];

        Effect = new WeatherWaterSourceBuffEffect(Value.Modifier, t);
        Effects = [Effect];

        OverrideName = t.T(IsBuff ? "LV.WSP.IncreaseStr" : "LV.WSP.DecreaseStr");
        OverrideDescription = t.T(IsBuff ? "LV.WSP.IncreaseStrBuffDesc" : "LV.WSP.DecreaseStrBuffDesc");
    }

}

public class WeatherWaterSourceBuffTarget(EntityManager entities, bool fresh, EventBus eb) : EntityBasedBuffTarget(eb)
{
    protected override bool Filter(EntityComponent entity) => entity.GetComponentFast<WeatherUpgradeWaterStrengthModifier>() is not null;

    protected override HashSet<BuffableComponent> GetTargets()
    {
        HashSet<BuffableComponent> targets = [.. entities.Get<WeatherUpgradeWaterStrengthModifier>()
            .AsEnumerable()
            .Where(q => q.IsBadwaterSource != fresh)
            .Select(q => q.GetBuffable())];

        return targets;
    }
}

public class WeatherWaterSourceBuffEffect(float value, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value) => string.Format(t.T("LV.WSP.StrBuffEffect"), value);
}