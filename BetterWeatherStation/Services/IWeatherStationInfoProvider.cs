namespace BetterWeatherStation.Services;

public record WeatherDefinition(string Id, string Name, bool Hazardous);
public readonly record struct CurrentWeatherStatus(
    WeatherDefinition Current,
    WeatherDefinition Next,
    float HoursUntilNext
);

public interface IWeatherStationInfoProvider
{
    int Order { get; }
    CurrentWeatherStatus Current { get; }

    /// <summary>
    /// Called once on initialization
    /// </summary>
    IReadOnlyList<WeatherDefinition> GetWeathers();

    /// <summary>
    /// Gets the default weather definition. This is only called after <see cref="GetWeathers"/> is called.
    /// </summary>
    WeatherDefinition GetDefaultWeather();

    void Update();

}
