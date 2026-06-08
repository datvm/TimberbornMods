
namespace BeaverChronicles.Services.Conditions;

public class GameStatData
{
    public string Stat { get; init; } = "";
    public float Value { get; init; }
    public NumericComparisonMode Comparison { get; init; } = NumericComparisonMode.Equal;
}

[MultiBind(typeof(IConditionEvaluator))]
public class GameStat(GameStatService gameStatService, EvaluationCacheService caches) : IConditionEvaluator
{
    public string ForType => "GameStat";

    public bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        var data = c.GetParameters<GameStatData>() ?? throw new InvalidDataException("No parameters provided for GameStat condition.");

        var cacheKey = $"{nameof(GameStat)}.{data.Stat}";
        var value = caches.GetOrEvaluate(cacheKey, () => gameStatService.GetStat(data.Stat));

        return value switch
        {
            int i => data.Comparison.Evaluate(i, (int)data.Value),
            float f => data.Comparison.Evaluate(f, data.Value),
            _ => throw new InvalidDataException("Only number stats are supported in GameStat condition."),
        };
    }

}
