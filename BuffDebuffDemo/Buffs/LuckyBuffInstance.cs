namespace BuffDebuffDemo.Buffs;

// Since this BuffInstance needs a few values, we create a record to hold them.
public readonly record struct LuckyBuffInstanceValue(float Hours, float Speed);

// This BuffInstance is timed so we use TimedBuffInstance which already has the logic for processing it
// We also implement IValuedBuffInstance<LuckyBuffInstanceValue> because TimedBuffInstance doesn't support it out of the box.
public class LuckyBuffInstance : TimedBuffInstance<LuckyBuff>, IValuedBuffInstance<LuckyBuffInstanceValue>
{
    // This is the total time when the buff starts. Make sure it's available before Init() call of the base class.
    public override float StartingTime => Value.Hours;

    // If you override AdditionalDescription, note that the base class already has a logic to show the time left.
    // public override string? AdditionalDescription { get => base.AdditionalDescription; protected set => base.AdditionalDescription = value; }

    public override bool IsBuff { get; protected set; } = true;

    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];
    public SpeedBuffEffect Effect { get; private set; } = null!;

    // These are the dependencies that the TimedBuffInstance needs to process it,
    // so we need to inject them as well beside what extra you may need
    protected override IBuffService Buffs { get; set; } = null!;
    protected override IDayNightCycle DayNight { get; set; } = null!;
    protected override ILoc T { get; set; } = null!;

    IBuffableService buffables = null!;
    EventBus eb = null!;

    // Property for IValuedBuffInstance<LuckyBuffInstanceValue> so the IBuffService can set it automatically
    public LuckyBuffInstanceValue Value { get; set; }

    [Inject]
    public void InjectDeps(IBuffService buffs, IDayNightCycle dayNight, ILoc t, IBuffableService buffables, EventBus eb)
    {
        // Beside injecting your own dependencies, you need to inject the base class dependencies as well
        base.Inject(buffs, dayNight, t);

        this.buffables = buffables;
        this.eb = eb;
    }

    public override void Init()
    {
        // This is needed as there is some logic in the base class
        base.Init();

        Targets = [new GlobalBeaverBuffTarget(buffables, eb)];
        Effect = new SpeedBuffEffect(T, Value.Speed);
        Effects = [Effect];
    }

}
