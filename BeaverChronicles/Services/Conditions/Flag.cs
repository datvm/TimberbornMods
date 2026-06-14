namespace BeaverChronicles.Services.Conditions;

public record FlagData
{
    public string? Flag { get; init; }
    public ImmutableArray<string> Flags { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.All;
}

[MultiBind(typeof(IConditionEvaluator))]
public class Flag : ConditionEvaluatorBase<FlagData>
{
    public override string ForType => nameof(Flag);

    protected override bool Evaluate(FlagData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        var flags = GetFlags();
        if (flags.Count == 0)
        {
            return false;
        }

        return p.ConditionType.Evaluate(flags, ev.Controller.HelperCollection.Flags.HasFlag);

        List<string> GetFlags()
        {
            List<string> result = [];
            if (p.Flag is not null)
            {
                result.Add(ev.Controller.FormatText(p.Flag));
            }

            result.AddRange(p.Flags.Select(ev.Controller.FormatText).Where(f => f is not null)!);
            return result;
        }
    }
}
