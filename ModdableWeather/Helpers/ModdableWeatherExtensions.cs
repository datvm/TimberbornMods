namespace ModdableWeather;

public static class ModdableWeatherExtensions
{

    public static IModdedHazardousWeather GetWeather(this HazardousWeatherStartedEvent ev) =>
       (IModdedHazardousWeather)ev.HazardousWeather;

    public static bool IsDrought(this IHazardousWeather weather) => weather is GameDroughtWeather;
    public static bool IsBadtide(this IHazardousWeather weather) => weather is GameBadtideWeather;
    

}
