namespace BeaverChronicles.Services.Conditions;

public record ChanceConditionData
{
    public string Value { get; init; } = "";
}

[MultiBind(typeof(IConditionEvaluator))]
public class Chance : ConditionEvaluatorBase<ChanceConditionData>
{
    public override string ForType => nameof(Chance);

    protected override bool Evaluate(ChanceConditionData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        return BeaverChroniclesUtils.Chance(ev.Controller.FormatTextFloat(p.Value));
    }
}
