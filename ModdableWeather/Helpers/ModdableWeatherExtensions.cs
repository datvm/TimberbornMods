namespace ModdableWeather;

public static class ModdableWeatherExtensions
{

    public static IModdableHazardousWeather GetWeather(this HazardousWeatherStartedEvent ev) =>
       (IModdableHazardousWeather)ev.HazardousWeather;

    public static bool IsDrought(this IHazardousWeather weather) => weather is GameDroughtWeather;
    public static bool IsBadtide(this IHazardousWeather weather) => weather is GameBadtideWeather;
    

}
