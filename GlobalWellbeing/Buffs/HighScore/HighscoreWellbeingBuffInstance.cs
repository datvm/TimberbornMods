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
    EventBus eb = null!;
    BeaverPopulation beaverPops = null!;

    [JsonProperty]
    public HighscoreWellbeingValue Value { get; set; }

    [Inject]
    public void Inject(EventBus eb, BeaverPopulation beaverPops)
    {
        this.eb = eb;
        this.beaverPops = beaverPops;
    }

    public override void Init()
    {
        base.Init();

        Targets = [new BeaverBuffTarget(eb, beaverPops)];

        Effect = new HighscoreWellbeingBuffEffect(T, Value.Speed);
        Effects = [Effect];
    }

}
