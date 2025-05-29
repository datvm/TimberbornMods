namespace ModdableWeather;

public static class ModdableWeatherExtensions
{

    public static IModdedHazardousWeather GetWeather(this HazardousWeatherStartedEvent ev) =>
       (IModdedHazardousWeather)ev.HazardousWeather;

}
