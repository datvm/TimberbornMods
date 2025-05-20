namespace ModdableWeather.Specs;

public interface IModdedWeather
{
    string Id { get; }

    public IModdedHazardousWeather? IsHazardous() => this as IModdedHazardousWeather;
    public ITemperateWeather? IsTemperate() => this as ITemperateWeather;

    int GetDurationAtCycle(int cycle);
    float GetProbability(int cycle);
}

public interface IModdedHazardousWeather : IModdedWeather, IHazardousWeather
{

}

public interface ITemperateWeather : IModdedWeather
{

}