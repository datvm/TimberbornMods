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
            if (!Matches(stats.GetStat<string>(GameStats.WeatherWarningNextWeather), hazardousWeatherId))
            {
                return false;
            }
        }

        if (p.CurrentWeatherId is { } currentWeatherId)
        {
            if (!Matches(stats.GetStat<string>(GameStats.WeatherCurrent), currentWeatherId))
            {
                return false;
            }
        }

        if (p.WarningStage is { } warningStage)
        {
            if (!Matches(stats.GetStat<string>(GameStats.WeatherWarningStage), warningStage))
            {
                return false;
            }
        }

        return p.HazardousWeatherId is not null || p.CurrentWeatherId is not null || p.WarningStage is not null;

        bool Matches(string? actual, string expected)
            => string.Equals(actual, ev.Controller.FormatText(expected), StringComparison.InvariantCultureIgnoreCase);
    }
}
