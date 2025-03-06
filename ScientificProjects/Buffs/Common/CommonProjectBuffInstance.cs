namespace ScientificProjects.Buffs;

public abstract class CommonProjectBuffInstance<TBuff> : BuffInstance<IEnumerable<ScientificProjectInfo>, TBuff>
    where TBuff : IBuff
{
    public override bool IsBuff { get; protected set; } = true;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];

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

        Targets = CreateTargets();
        Effects = Value.Select(CreateBuffEffect);
    }

    protected abstract IBuffEffect CreateBuffEffect(ScientificProjectInfo info);
    protected abstract IBuffTarget[] CreateTargets();

    protected override string? Save() => null; // Usually there is no saving for this since it's created from the project info
    protected override bool Load(string savedState) => false; // Usually there is no saving for this since it's created from the project info
}

public abstract class CommonProjectBeaverBuffInstance<TBuff> : CommonProjectBuffInstance<TBuff>
    where TBuff : IBuff
{

    protected override IBuffTarget[] CreateTargets() => [new GlobalBeaverBuffTarget(buffables, ev)];

}