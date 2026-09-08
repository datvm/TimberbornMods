namespace BuffDebuffDemo.Buffs;

// See guide at PositiveBuffInstance.cs
public class NegativeBuffInstance : SimpleFloatBuffInstance<NegativeBuff>
{
    public override bool IsBuff { get; protected set; } = false;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];
    public SpeedBuffEffect Effect { get; private set; } = null!;
    IBuffableService buffables = null!;
    EventBus eb = null!;
    ILoc t = null!;

    [Inject]
    public void Inject(IBuffableService buffables, EventBus eb, ILoc t)
    {
        this.buffables = buffables;
        this.eb = eb;
        this.t = t;
    }

    public override void Init()
    {
        Targets = [new GlobalBeaverBuffTarget(buffables, eb)];
        Effect = new SpeedBuffEffect(t, Value);
        Effects = [Effect];
    }
}
