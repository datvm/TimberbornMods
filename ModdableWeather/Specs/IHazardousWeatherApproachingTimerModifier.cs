namespace ModdableWeather.Specs;

public interface IHazardousWeatherApproachingTimerModifier
{

    int Order { get; }

    int Modify(int current, int original);

}
