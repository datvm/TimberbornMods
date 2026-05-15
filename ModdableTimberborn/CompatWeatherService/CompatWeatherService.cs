namespace ModdableTimberborn.CompatWeatherService;

[BindSingleton]
public class CompatWeatherService(IEnumerable<ICompatWeatherServiceProvider> providers)
{
    public const string TemperateId = "TemperateWeather";
    public const string DroughtId = "DroughtWeather";
    public const string BadtideId = "BadtideWeather";

    public readonly ICompatWeatherServiceProvider Provider = providers.OrderByDescending(p => p.Priority).FirstOrDefault()
        ?? throw new InvalidOperationException("No ICompatWeatherServiceProvider implementations found.");

}
