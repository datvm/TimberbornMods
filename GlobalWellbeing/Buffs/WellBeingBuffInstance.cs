namespace GlobalWellbeing.Buffs;

public class WellBeingBuffInstance(WellBeingBuff buff, int wellbeing, bool forKid) : SimpleValueBuffInstance<int, WellBeingBuff, WellBeingBuffInstance>(buff, wellbeing)
{
    ImmutableArray<IBuffTarget> targets = [];
    ImmutableArray<IBuffEffect> effects = [];

    int wellbeing = wellbeing;

    public override IEnumerable<IBuffTarget> Targets => targets;
    public override IEnumerable<IBuffEffect> Effects => effects;

    public override bool IsBuff { get; protected set; } = wellbeing >= 0;

    [Inject]
    public void Inject(IBuffableService buffable, EventBus eventBus, IWellbeingTierService itier, ILoc t)
    {
        if (itier is not WellbeingTierService tier)
        {
            throw new NotSupportedException($"Game has been updated ({nameof(IWellbeingTierService)} is no longer {nameof(WellbeingTierService)})." +
                $" Please wait for this mod to be updated");
        }

        targets = [forKid 
            ? new GlobalChildBeaverBuffTarget(buffable, eventBus)
            : new GlobalBeaverBuffTarget(buffable, eventBus)
        ];

        var bonuses = Process(forKid ?
            tier._childWellbeingTiers :
            tier._adultWellbeingTiers,
            wellbeing);
        effects = bonuses
            .Select(bonus => new WellBeingEffect(t, bonus))
            .ToImmutableArray<IBuffEffect>();
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

}
