namespace BeaverChronicles.Services.Conditions;

public record WeatherData
{
    public string? HazardousWeatherId { get; init; }
    public string? CurrentWeatherId { get; init; }
    public string? WarningStage { get; init; }
}

[MultiBind(typeof(IConditionEvaluator))]
public class Weather : ConditionEvaluatorBase<WeatherData>
{
    public override string ForType => nameof(Weather);

    protected override bool Evaluate(WeatherData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        if (p is null) { throw ThrowMissingData(ForType); }

        var stats = ev.Controller.HelperCollection.GameStats;

        if (p.HazardousWeatherId is { } hazardousWeatherId)
        {
            if (!Matches(stats.GetStat<string>(GameStats.WeatherWarningNextWeather), hazardousWeatherId, nameof(p.HazardousWeatherId)))
            {
                return false;
            }
        }

        if (p.CurrentWeatherId is { } currentWeatherId)
        {
            if (!Matches(stats.GetStat<string>(GameStats.WeatherCurrent), currentWeatherId, nameof(p.CurrentWeatherId)))
            {
                return false;
            }
        }

        if (p.WarningStage is { } warningStage)
        {
            if (!Matches(stats.GetStat<string>(GameStats.WeatherWarningStage), warningStage, nameof(p.WarningStage)))
            {
                return false;
            }
        }

        return p.HazardousWeatherId is not null || p.CurrentWeatherId is not null || p.WarningStage is not null;

        bool Matches(string? actual, string expected, string name)
        {
            var expectedValue = ev.Controller.FormatText(expected);
            var match = string.Equals(actual, expectedValue, StringComparison.InvariantCultureIgnoreCase);

            this.LogVerbose(node, () => $"- {name}: expected {expectedValue}, actual {actual} -> Evaluated to {match}");

            return match;
        }
    }
}
