
namespace BeaverChronicles.Services.Conditions;

public record GameStatData(
    string Stat,
    string Value,
    NumericComparisonMode Comparison = NumericComparisonMode.Equal
);

[MultiBind(typeof(IConditionEvaluator))]
public class GameStat(GameStatHelper helper) : ConditionEvaluatorBase<GameStatData>
{
    public override string ForType => "GameStat";

    protected override bool Evaluate(GameStatData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        var actualValue = helper.GetStat(p.Stat);
        var requestedValue = ev.Controller.FormatTextFloat(p.Value);

        return actualValue switch
        {
            int i => p.Comparison.Evaluate(i, (int)requestedValue),
            float f => p.Comparison.Evaluate(f, requestedValue),
            null => false,
            _ => throw new InvalidDataException("Only number stats are supported in GameStat condition. Received: " + actualValue.GetType().FullName),
        };
    }
}
