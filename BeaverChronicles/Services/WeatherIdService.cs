namespace BeaverChronicles.Services;

[BindSingleton]
public class WeatherIdService
{
    public const string TemperateId = "TemperateWeather";
    public const string DroughtId = "DroughtWeather";
    public const string BadtideId = "BadtideWeather";

    readonly MethodInfo? moddableCurrentWeatherGetter;
    readonly MethodInfo? moddableWeatherIdGetter;
    readonly WeatherService weatherService;
    readonly HazardousWeatherService hazardousWeatherService;
    readonly IDayNightCycle cycle;

    string? cachedId;
    int cachedDay;

    public WeatherIdService(WeatherService weatherService, HazardousWeatherService hazardousWeatherService, IDayNightCycle cycle)
    {
        this.weatherService = weatherService;
        this.hazardousWeatherService = hazardousWeatherService;
        this.cycle = cycle;

        moddableCurrentWeatherGetter = weatherService.GetType().PropertyGetter("CurrentWeather");
        if (moddableCurrentWeatherGetter != null)
        {
            moddableWeatherIdGetter = moddableCurrentWeatherGetter.ReturnType.PropertyGetter("Id")
                ?? throw new ArgumentException("CurrentWeather type does not have an Id property.");
        }

    }

    public string GetId()
    {
        if (cachedId is not null && cachedDay == cycle.DayNumber)
        {
            return cachedId;
        }

        if (moddableCurrentWeatherGetter is null)
        {
            cachedId = weatherService.IsHazardousWeather
                ? hazardousWeatherService.CurrentCycleHazardousWeather.Id
                : TemperateId;
        }
        else
        {
            var weather = moddableCurrentWeatherGetter.Invoke(weatherService, []);
            cachedId = moddableWeatherIdGetter!.Invoke(weather, []) as string ?? TemperateId;
        }

        cachedDay = cycle.DayNumber;
        return cachedId;
    }

}
