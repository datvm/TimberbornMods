namespace ScientificProjects.Buffs;

public abstract class CommonProjectBuffInstance<TBuff> : BuffInstance<IEnumerable<ScientificProjectInfo>, TBuff>
    where TBuff : CommonProjectsBuff
{
    public override bool IsBuff { get; protected set; } = true;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];

    protected abstract string GetBuffName(ILoc t);
    protected abstract string GetBuffDescription(ILoc t);

    protected ILoc t = null!;
    protected IBuffableService buffables = null!;
    protected EventBus ev = null!;

    [Inject]
    public void CommonProjectBuffInstanceInject(ILoc t, IBuffableService buffables, EventBus ev)
    {
        this.t = t;
        this.buffables = buffables;
        this.ev = ev;
    }

    public override void Init()
    {
        base.Init();

        OverrideName = GetBuffName(t);
        OverrideDescription = GetBuffDescription(t);

        Targets = CreateTargets();
        Effects = [..Value.Select(CreateBuffEffect)
            .Where(q => q is not null)!];
    }

    protected abstract IBuffEffect? CreateBuffEffect(ScientificProjectInfo info);
    protected abstract IBuffTarget[] CreateTargets();

    protected override string? Save() => null; // Usually there is no saving for this since it's created from the project info
    protected override bool Load(string savedState) => false; // Usually there is no saving for this since it's created from the project info

    // Common Helpers for child classes

    protected static TEff? CreateFlatEffect<TEff>(ScientificProjectInfo info, Func<float, string, TEff> effFunc, int valueParamIndex = 0) where TEff : IBuffEffect
    {
        return info.Spec.HasSteps ? default : effFunc(info.Spec.Parameters[valueParamIndex], info.Spec.DisplayName);
    }

    protected static TEff? CreateLevelEffect<TEff>(ScientificProjectInfo info, Func<float, string, TEff> effFunc, int valueParamIndex = 0) where TEff : IBuffEffect
    {
        return info.Spec.HasSteps ? effFunc(info.Spec.Parameters[valueParamIndex] * info.TodayLevel, info.TodayName) : default;
    }

    protected static TEff CreateFlatOrLevelEffect<TEff>(ScientificProjectInfo info, Func<float, string, TEff> effFunc, int valueParamIndex = 0) where TEff : IBuffEffect
    {
        var s = info.Spec;

        var value = s.Parameters[valueParamIndex];
        var name = s.DisplayName;

        if (s.HasSteps)
        {
            value *= info.TodayLevel;
            name = info.TodayName;
        }

        return effFunc(value, name);
    }
}

public abstract class CommonProjectBeaverBuffInstance<TBuff> : CommonProjectBuffInstance<TBuff>
    where TBuff : CommonProjectsBuff
{
    protected BeaverPopulation beaverPops = null!;
    protected virtual BeaverTarget Target => BeaverTarget.All;

    [Inject]
    public void CommonProjectBeaverBuffInstanceInject(BeaverPopulation beaverPops)
    {
        this.beaverPops = beaverPops;
    }

    protected override IBuffTarget[] CreateTargets() => [
        Target switch
        {
            BeaverTarget.All => new BeaverBuffTarget(ev, beaverPops),
            BeaverTarget.Adult => new AdultBeaverBuffTarget(ev, beaverPops),
            BeaverTarget.Child => new ChildBeaverBuffTarget(ev, beaverPops),
            _ => throw new ArgumentOutOfRangeException()
        }
    ];

}

public enum BeaverTarget
{
    All = 0,
    Adult = 1,
    Child = 2,
}