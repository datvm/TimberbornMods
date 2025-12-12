namespace ModdableWeather.HazardousTimer;

public interface IHazardousWeatherApproachingTimerModifier
{

    int Order => 0;
    bool Disabled { get; }

    int Modify(int current, int original);
    event Action OnChanged;

}
