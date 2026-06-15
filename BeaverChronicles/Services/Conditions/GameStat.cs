
namespace BeaverChronicles.Services.Conditions;

public record GameStatData(
    string Stat,
    string Value,
    bool CheckExistsOnly = false,
    NumericComparisonMode Comparison = NumericComparisonMode.Equal
);

[MultiBind(typeof(IConditionEvaluator))]
public class GameStat(GameStatHelper helper) : ConditionEvaluatorBase<GameStatData>
{
    public override string ForType => "GameStat";

    protected override bool Evaluate(GameStatData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        var stat = ev.Controller.FormatText(p.Stat);
        if (p.CheckExistsOnly)
        {
            return helper.HasStat(stat);
        }

        var actualValue = helper.GetStat(stat);
        var requestedValue = ev.Controller.FormatTextFloat(p.Value);

        var result = actualValue switch
        {
            int i => p.Comparison.Evaluate(i, (int)requestedValue),
            float f => p.Comparison.Evaluate(f, requestedValue),
            bool b => p.Comparison.Evaluate(b ? 1 : 0, (int)requestedValue),
            null => false,
            _ => throw new InvalidDataException("Only number and boolean stats are supported in GameStat condition. Received: " + actualValue.GetType().FullName),
        };

        this.LogVerbose(node, () => $"Stat '{stat}' has value {actualValue}, compare {p.Comparison} with {requestedValue} -> Evaluated to {result}");

        return result;
    }
}
