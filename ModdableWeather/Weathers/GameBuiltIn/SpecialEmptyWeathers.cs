namespace ModdableWeather.Weathers;

public abstract class SpecialEmptyWeather : ModdableWeatherBase
{
    public SpecialEmptyWeather()
    {
        Enabled = false;
    }

    public override int GetChance(int cycle, ModdableWeatherHistoryProvider history) => 0;
    public override int GetDurationAtCycle(int cycle, ModdableWeatherHistoryProvider history) => 0;
}

public class EmptyHazardousWeather : SpecialEmptyWeather, IModdableHazardousWeather
{
    public override string Id { get; } = nameof(EmptyHazardousWeather);
}

public class EmptyBenignWeather : SpecialEmptyWeather, IModdableBenignWeather
{
    public override string Id { get; } = nameof(EmptyBenignWeather);
}