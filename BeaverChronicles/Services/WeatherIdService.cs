namespace BeaverChronicles.Services;

public interface IWeatherIdService
{
    string GetWeatherId();
}

[BindSingleton(As = typeof(IWeatherIdService))]
public class WeatherIdService(WeatherService weatherService, HazardousWeatherService hazardousWeatherService) : IWeatherIdService
{
    public string GetWeatherId() => weatherService.IsHazardousWeather
        ? hazardousWeatherService.CurrentCycleHazardousWeather.Id
        : "TemperateWeather";
}
