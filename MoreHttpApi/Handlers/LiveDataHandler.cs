namespace MoreHttpApi.Handlers;

[MultiBind(typeof(IMoreHttpApiHandler))]
public class LiveDataHandler(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService,
    WeatherService weatherService,
    HazardousWeatherService hazardousWeatherService,
    HazardousWeatherApproachingTimer timer,
    SpeedManager speedManager,
    ILoc t
) : IMoreHttpApiHandler
{
    public string Endpoint { get; } = "live-data";

    public async Task<bool> HandleAsync(HttpListenerContext context, ParsedRequestPath parsedRequestPath)
        => parsedRequestPath.RemainingSegment.Length switch
        {
            0 => await context.HandleAsync(GetCommonDataAsync),
            1 => parsedRequestPath.RemainingSegment[0] switch
            {
                "weather-data" => await context.HandleAsync(GetAllWeathers),
                "set-game-speed" => await context.HandleAsync(() => SetGameSpeed(parsedRequestPath)),
                _ => false
            },
            _ => false,
        };

    async Task<HttpWeatherSpec[]> GetAllWeathers() => [
        new(HttpWeatherSpec.TemperateWeatherId, t.T("Weather.Temperate"), "UI/Images/Core/ico-weather-temperate"),
        new(HttpWeatherSpec.DroughtWeatherId, t.T("Weather.Drought"), "UI/Images/Core/ico-weather-drought"),
        new(HttpWeatherSpec.BadtideWeatherId, t.T("Weather.Badtide"), "UI/Images/Core/ico-weather-badtide"),
    ];

    async Task<HttpCommonData> GetCommonDataAsync()
    {
        var cycle = GetCycle();

        return new(new(
            cycle,
            GetWeather(cycle),
            GetGameSpeed()
        ));
    }

    HttpCycle GetCycle() => new(gameCycleService.Cycle, gameCycleService.CycleDay, dayNightCycle.DayProgress);

    HttpCurrentWeather GetWeather(HttpCycle cycle)
    {
        var isHazardous = weatherService.IsHazardousWeather;

        HttpCurrentWeather info;
        HttpNextWeather next;

        if (isHazardous)
        {
            var remaining = GetRemainingDays(weatherService.CycleLengthInDays + 1);
            next = new(HttpWeather.Temperate, remaining);
            info = new(new(hazardousWeatherService.CurrentCycleHazardousWeather.Id, weatherService.HazardousWeatherStartCycleDay), next, true);
        }
        else
        {
            var hasHaz = hazardousWeatherService.HazardousWeatherDuration > 0;
            var shouldShow = false;
            if (hasHaz)
            {
                var remaining = GetRemainingDays(weatherService.HazardousWeatherStartCycleDay);
                next = new(new(hazardousWeatherService.CurrentCycleHazardousWeather.Id, weatherService.HazardousWeatherStartCycleDay), remaining);
                shouldShow = timer.GetProgress() > 0f;
            }
            else
            {
                var remaining = GetRemainingDays(weatherService.CycleLengthInDays);
                next = new(HttpWeather.Temperate, remaining);
            }

            info = new(HttpWeather.Temperate, next, shouldShow);
        }

        return info;

        float GetRemainingDays(int totalDays) => totalDays - cycle.Day - cycle.Hours;
    }

    HttpGameSpeed GetGameSpeed() => new(speedManager.CurrentSpeed, speedManager._isLocked);

    public async Task SetGameSpeed(ParsedRequestPath parsedRequestPath)
    {
        var speedStr = parsedRequestPath.QueryParameters["speed"];
        if (!float.TryParse(speedStr, out var speed))
        {
            throw new StatusCodeException(400, "Invalid speed value");
        }

        speedManager.ChangeSpeed(speed);
    }

}