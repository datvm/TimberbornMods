namespace MoreHttpApi.Shared;

public record HttpCycle(
    int Cycle,
    int Day,
    float Hours
);

public record HttpCurrentWeather(
    HttpWeather Current,
    HttpNextWeather? Next,
    bool ShouldShowNext
);

public record HttpWeather(
    string Id,
    int StartDay
)
{
    public static readonly HttpWeather Temperate = new(HttpWeatherSpec.TemperateWeatherId, 0);
}

public record HttpNextWeather(
    HttpWeather Weather,
    float ComingInDays
);

public record HttpGameSpeed(
    float Speed,
    bool Locked
);

public record HttpWeatherSpec(
    string Id,
    string Name,
    string Icon
)
{
    public const string TemperateWeatherId = "TemperateWeather";
    public const string DroughtWeatherId = "DroughtWeather";
    public const string BadtideWeatherId = "BadtideWeather";
}