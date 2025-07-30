namespace WeatherEditor.Services;

public class GameWeatherModService(
    TemperateWeatherDurationService temperateWeatherDurationService,
    HazardousWeatherService hazardousWeatherService,
    ILoc t,
    BadtideWeather badtideWeather,
    DroughtWeather droughtWeather
) : IWeatherModService
{
    public bool IsModdableWeather { get; } = false;

    public int TemperateWeatherDuration
    {
        get => temperateWeatherDurationService.TemperateWeatherDuration;
        set => temperateWeatherDurationService.TemperateWeatherDuration = value;
    }

    public int HazardousWeatherDuration
    {
        get => hazardousWeatherService.HazardousWeatherDuration;
        set => hazardousWeatherService.HazardousWeatherDuration = value;
    }

    public ImmutableArray<HazardousWeatherInfo> HazardousWeathers { get; } = [
        new(droughtWeather.Id, t.T("Weather.Drought"), droughtWeather),
        new(badtideWeather.Id, t.T("Weather.Badtide"), badtideWeather)
    ];

    public IHazardousWeather CurrentCycleHazardousWeather
    {
        get => hazardousWeatherService.CurrentCycleHazardousWeather;
        set => hazardousWeatherService.CurrentCycleHazardousWeather = value;
    }

}
