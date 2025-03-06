namespace ScientificProjects.Buffs;

public class MovementSpeedUpgradeBuffInstance : BuffInstance<IEnumerable<ScientificProjectSpec>, MovementSpeedUpgradeBuff>
{
    public override bool IsBuff { get; protected set; } = true;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];

    ILoc t = null!;
    IBuffableService buffables = null!;
    EventBus ev = null!;

    [Inject]
    public void Inject(ILoc t, IBuffableService buffables, EventBus ev)
    {
        this.t = t;
        this.buffables = buffables;
        this.ev = ev;
    }

    public override void Init()
    {
        base.Init();

        Targets = [new GlobalBeaverBuffTarget(buffables, ev)];
        Effects = Value.Select(q => new MovementSpeedBuffEffect(q.Parameters[0], q.DisplayName, t));
    }

    protected override string? Save() => null; // No saving for this
    protected override bool Load(string savedState) => false; // No saving for this

}
