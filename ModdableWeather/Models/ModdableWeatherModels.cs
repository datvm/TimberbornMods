namespace ModdableWeather.Models;

public readonly record struct CycleWeatherPair(IModdedTemperateWeather Temperate, IModdedHazardousWeather Hazadous);

[Flags]
public enum WeatherDifficulty
{
    None = 0,
    Easy = 1,
    Normal = 2,
    Hard = 4,
    All = 1 | 2 | 4,
}