namespace ModdableWeathers.Weathers.GameBuiltIns;

public abstract class EmptyWeather : ModdableWeatherBase
{
    protected EmptyWeather(ModdableWeatherSpecService specs) : base(specs)
    {
        Enabled = false;
    }

    public override int GetChance(int cycle, WeatherHistoryService history) => 0;
    public override int GetDurationAtCycle(int cycle, WeatherHistoryService history) => 0;
}

public class EmptyBenignWeather(ModdableWeatherSpecService specs) : EmptyWeather(specs), IModdableBenignWeather
{
    public const string WeatherId = nameof(EmptyWeather);
    public override string Id { get; } = WeatherId;
}

public class EmptyHazardousWeather(ModdableWeatherSpecService specs) : EmptyWeather(specs), IModdableHazardousWeather
{
    public const string WeatherId = nameof(EmptyWeather);
    public override string Id { get; } = WeatherId;
}

