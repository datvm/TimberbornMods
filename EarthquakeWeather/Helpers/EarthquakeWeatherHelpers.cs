namespace EarthquakeWeather.Helpers;

public static class EarthquakeWeatherHelpers
{

    public static EarthquakeComponent GetEarthquakeComponent<T>(this T component)
        where T : BaseComponent => component.GetComponentFast<EarthquakeComponent>();

}
