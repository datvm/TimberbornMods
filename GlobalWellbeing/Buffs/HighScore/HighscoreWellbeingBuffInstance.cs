using Newtonsoft.Json;

namespace GlobalWellbeing.Buffs.HighScore;

public readonly record struct HighscoreWellbeingValue(float Hours, float Speed);

public class HighscoreWellbeingBuffInstance : TimedBuffInstance<HighscoreWellbeingBuff>, IValuedBuffInstance<HighscoreWellbeingValue>
{
    public HighscoreWellbeingBuffEffect Effect { get; private set; } = null!;

    public override bool IsBuff { get; protected set; } = true;
    public override IEnumerable<IBuffTarget> Targets { get; protected set; } = [];
    public override IEnumerable<IBuffEffect> Effects { get; protected set; } = [];
    public override float StartingTime => Value.Hours;

    protected override IBuffService Buffs { get; set; } = null!;
    protected override IDayNightCycle DayNight { get; set; } = null!;
    protected override ILoc T { get; set; } = null!;
    IBuffableService buffables = null!;
    EventBus eb = null!;

    [JsonProperty]
    public HighscoreWellbeingValue Value { get; set; }

    [Inject]
    public void Inject(IBuffService buffs, IDayNightCycle dayNight, ILoc t, IBuffableService buffables, EventBus eb)
    {
        base.Inject(buffs, dayNight, t);
        this.buffables = buffables;
        this.eb = eb;
    }

    public override void Init()
    {
        base.Init();

        Targets = [new GlobalBeaverBuffTarget(buffables, eb)];

        Effect = new HighscoreWellbeingBuffEffect(T, Value.Speed);
        Effects = [Effect];
    }

}
