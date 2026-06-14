namespace BeaverChronicles.Services.Conditions;

public record CustomParameterData
{
    public string Key { get; init; } = "";
    public string? Value { get; init; }
    public NumericComparisonMode Comparison { get; init; } = NumericComparisonMode.Equal;
}

[MultiBind(typeof(IConditionEvaluator))]
public class _CustomParameter : ConditionEvaluatorBase<CustomParameterData>
{
    public override string ForType => nameof(_CustomParameter);

    protected override bool Evaluate(CustomParameterData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        var key = ev.Controller.FormatText(p.Key);
        var exists = ev.Controller.CurrentRecord.CustomParameters.TryGetValue(key, out var actualValue);

        return p.Value is null
            ? exists
            : exists && Compare(actualValue!, ev.Controller.FormatText(p.Value), p.Comparison);
    }

    static bool Compare(string actualValue, string? requestedValue, NumericComparisonMode comparison)
    {
        requestedValue ??= "";

        if (float.TryParse(actualValue, out var actualNumber)
            && float.TryParse(requestedValue, out var requestedNumber))
        {
            return comparison.Evaluate(actualNumber, requestedNumber);
        }

        return comparison == NumericComparisonMode.Equal
            && string.Equals(actualValue, requestedValue, StringComparison.InvariantCultureIgnoreCase);
    }
}
