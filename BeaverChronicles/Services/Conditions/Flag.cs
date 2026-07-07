namespace BeaverChronicles.Services.Conditions;

public record FlagData
{
    public string? Flag { get; init; }
    public ImmutableArray<string> Flags { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.All;
    public bool State { get; init; } = true;
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
            this.LogVerbose(node, () => "- No flags specified, condition automatically fails.");
            return false;
        }

        var result = p.ConditionType.Evaluate(flags, f => {
            var hasFlag = ev.Controller.HelperCollection.Flags.HasFlag(f);
            var matches = hasFlag == p.State;
            this.LogVerbose(node, () => $"- Flag {f}: {hasFlag}, expected {p.State} -> {matches}");
            return matches;
        });
        return result;

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
