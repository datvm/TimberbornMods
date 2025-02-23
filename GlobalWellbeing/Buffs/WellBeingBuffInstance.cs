namespace GlobalWellbeing.Buffs;

public readonly record struct WellBeingBuffInstanceData(int Wellbeing, bool ForKid);

public class WellBeingBuffInstance : BuffInstance<WellBeingBuffInstanceData, WellBeingBuff>
{
    ImmutableArray<IBuffTarget> targets = [];
    ImmutableArray<IBuffEffect> effects = [];

    public override IEnumerable<IBuffTarget> Targets => targets;
    public override IEnumerable<IBuffEffect> Effects => effects;

    public override bool IsBuff { get; protected set; }

    IBuffableService buffable = null!;
    EventBus eventBus = null!;
    WellbeingTierService tier = null!;
    ILoc t = null!;
    bool invalidState = false;

    [Inject]
    public void Inject(IBuffableService buffable, EventBus eventBus, IWellbeingTierService itier, ILoc t)
    {
        if (invalidState) { return; }

        Debug.Log("Injecting to buff instance");

        if (itier is not WellbeingTierService tier)
        {
            throw new NotSupportedException($"Game has been updated ({nameof(IWellbeingTierService)} is no longer {nameof(WellbeingTierService)})." +
                $" Please wait for this mod to be updated");
        }

        this.buffable = buffable;
        this.eventBus = eventBus;
        this.tier = tier;
        this.t = t;
    }

    public override void Init()
    {
        if (invalidState) { return; }

        Debug.Log("Init buff instance");

        base.Init();

        IsBuff = Value.Wellbeing >= 0;

        targets = [Value.ForKid
            ? new GlobalChildBeaverBuffTarget(buffable, eventBus)
            : new GlobalAdultBeaverBuffTarget(buffable, eventBus)
        ];

        var bonuses = Process(Value.ForKid ?
            tier._childWellbeingTiers :
            tier._adultWellbeingTiers,
            Value.Wellbeing);
        effects = [.. bonuses.Select(bonus => new WellBeingEffect(t, bonus))];
    }

    IEnumerable<WellBeingEffectStatPair> Process(Dictionary<string, WellbeingTier> dict, int wellbeing)
    {
        foreach (var (key, value) in dict)
        {
            if (!value.TryGetTierBonus(wellbeing, out var bonus)) { continue; }
            var hasNext = value.TryGetNextTierBonus(wellbeing, out var next);

            yield return new WellBeingEffectStatPair(
                new(key, bonus.Bonus, bonus.Wellbeing),
                hasNext ? new(key, next.Bonus, next.Wellbeing) : null
            );
        }
    }

    protected override string? Save() => null; // No need to save this, the buff can simply create a new instance on load

    protected override bool Load(string savedState) => false;

}
