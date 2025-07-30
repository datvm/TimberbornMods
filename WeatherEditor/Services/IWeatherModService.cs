

namespace WeatherEditor.Services;

public interface IWeatherModService
{
    public bool IsModdableWeather => this is IModdableWeatherModService;

    int TemperateWeatherDuration { get; set; }
    int HazardousWeatherDuration { get; set; }
    ImmutableArray<HazardousWeatherInfo> HazardousWeathers { get; }
    IHazardousWeather CurrentCycleHazardousWeather { get; set; }
}

public interface IModdableWeatherModService : IWeatherModService
{
    string NextTemperateWeatherId { get; set; }
    ModdableWeatherNextCycleWeather NextCycle { get; set; }
    bool IsSingleWeather { get; set; }
    bool IsSingleWeatherTemperate { get; set; }
    string CurrentTemperateWeatherId { get; set; }
    ImmutableArray<WeatherInfo> TemperateWeathers { get; }
    string CurrentTemperateWeatherName { get; }
    string NextTemperateWeatherName { get; }
}

public record WeatherInfo(string Id, string DisplayName);
public record HazardousWeatherInfo(string Id, string DisplayName, IHazardousWeather HazardousWeather);