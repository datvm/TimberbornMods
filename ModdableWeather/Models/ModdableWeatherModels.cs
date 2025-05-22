namespace ModdableWeather.Models;

public readonly record struct CycleWeatherPair(IModdedTemperateWeather Temperate, IModdedHazardousWeather Hazadous);