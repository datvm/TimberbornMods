namespace BetterWeatherStation.Services;

[MultiBind(typeof(IWeatherStationInfoProvider))]
public class DefaultWeatherStationInfoProvider(
    ILoc t,
    GameCycleService gameCycleService,
    WeatherService weatherService,
    HazardousWeatherService hazardousWeatherService
) : IWeatherStationInfoProvider
{
    public int Order => 0;
    public CurrentWeatherStatus Current { get; private set; }

    public const string TemperateWeatherId = "TemperateWeather";
    public static readonly string DroughtWeatherId = DroughtWeather.DroughtWeatherKey.Name;
    public static readonly string BadtideWeatherId = BadtideWeather.BadtideWeatherKey.Name;

#nullable disable
    WeatherDefinition Temperate, Drought, Badtide;
#nullable enable
    public WeatherDefinition GetDefaultWeather() => Temperate;

    public IReadOnlyList<WeatherDefinition> GetWeathers()
    {
        Temperate = new(TemperateWeatherId, t.T("Weather.Temperate"), false);
        Drought = new(DroughtWeatherId, t.T("Weather.Drought"), true);
        Badtide = new(BadtideWeatherId, t.T("Weather.Badtide"), true);

        return [Temperate, Drought, Badtide];
    }

    public void Update()
    {
        var days = gameCycleService.PartialCycleDay;
        var isHazardous = weatherService.IsHazardousWeather;

        WeatherDefinition curr, next;
        float remainingDays;

        if (isHazardous)
        {
            curr = CurrentHazardousWeather;
            next = Temperate;
            remainingDays = weatherService.CycleLengthInDays - days;
        }
        else
        {
            curr = Temperate;
            remainingDays = weatherService.HazardousWeatherStartCycleDay - days;

            var isNextTemperate = hazardousWeatherService.HazardousWeatherDuration <= 0;
            next = isNextTemperate ? curr : CurrentHazardousWeather;
        }

        Current = new(curr, next, remainingDays * 24f);
    }

    WeatherDefinition CurrentHazardousWeather
        => hazardousWeatherService.CurrentCycleHazardousWeather.Id == DroughtWeatherId
            ? Drought
            : Badtide;

}
