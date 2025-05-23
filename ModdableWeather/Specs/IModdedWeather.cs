namespace ModdableWeather.Specs;

public interface IModdedWeather
{
    string Id { get; }
    ModdedWeatherSpec Spec { get; set; }

    public IModdedHazardousWeather? IsHazardous() => this as IModdedHazardousWeather;
    public IModdedTemperateWeather? IsTemperate() => this as IModdedTemperateWeather;

    int GetDurationAtCycle(int cycle, ModdableWeatherService service);
    int GetChance(int cycle, ModdableWeatherService service);
}
