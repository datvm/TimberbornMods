namespace ModdableWeather.Defaults;

public class NoneHazardousWeather : DefaultHazardousWeather
{

    public override string Id { get; } = nameof(NoneHazardousWeather);
    public override WeatherParameters Parameters { get; } = new(true, 0, 0, 0, 0, 0, 0);

}
