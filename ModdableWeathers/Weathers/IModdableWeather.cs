namespace ModdableWeathers.Weathers;

public interface IModdableWeather
{
    string Id { get; }
    ModdableWeatherSpec Spec { get; }

    bool IsBenign => this is IModdableBenignWeather;
    bool IsHazardous => this is IModdableHazardousWeather;

    bool Active { get; }
    bool Enabled { get; }

    event WeatherChangedEventHandler? WeatherChanged;

    void End();
    void Start(bool onLoad);
}

public interface IModdableBenignWeather : IModdableWeather { }

public interface IModdableHazardousWeather : IModdableWeather, IHazardousWeather
{

    // Should not be needed anymore
    int IHazardousWeather.GetDurationAtCycle(int cycle) => throw new NotImplementedException();

}