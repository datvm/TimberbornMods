namespace ModdableWeather.Models;

public readonly record struct CycleWeatherPair(IModdableBenignWeather Temperate, IModdableHazardousWeather Hazadous);
