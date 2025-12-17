
namespace ModdableWeathers.Weathers.GameBuiltIns;

public abstract class EmptyWeather : ModdableWeatherBase
{
    protected EmptyWeather(ModdableWeatherSpecService specs) : base(specs)
    {
        Enabled = false;
    }

    public override int GetChance(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history)
        => 0;

    public override int GetDuration(WeatherCycleStageDecision stageDecision, WeatherCycleDecision cycleDecision, WeatherHistoryService history)
        => 0;
}

public class EmptyBenignWeather(ModdableWeatherSpecService specs) : EmptyWeather(specs), IModdableBenignWeather
{
    public const string WeatherId = nameof(EmptyBenignWeather);
    public override string Id { get; } = WeatherId;
}

public class EmptyHazardousWeather(ModdableWeatherSpecService specs) : EmptyWeather(specs), IModdableHazardousWeather
{
    public const string WeatherId = nameof(EmptyHazardousWeather);
    public override string Id { get; } = WeatherId;
}

