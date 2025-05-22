namespace ModdableWeather.Defaults;

public abstract class DefaultHazardousWeather : IModdedHazardousWeather
{
    public abstract string Id { get; }
    public abstract WeatherParameters Parameters { get; }

    public int GetDurationAtCycle(int cycle, ModdableWeatherService service)
    {
        throw new NotImplementedException();
    }

    public int GetChance(int cycle, ModdableWeatherService service)
    {
        throw new NotImplementedException();
    }
}

public abstract class DefaultHazardousWeather<TSettings>(TSettings settings) : DefaultHazardousWeather, IModdedHazardousWeather
    where TSettings : DefaultWeatherSettings
{

    protected TSettings Settings { get; } = settings;
    public override WeatherParameters Parameters => Settings.Parameters;

}