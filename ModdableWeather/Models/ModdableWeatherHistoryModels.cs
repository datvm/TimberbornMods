namespace ModdableWeather.Models;

public record ModdableWeatherCycle(int Cycle, ModdableWeatherCycleWeather TemperateWeather, ModdableWeatherCycleWeather HazardousWeather)
{
    public int HazardousWeatherDuration => HazardousWeather.Duration;
    public int TemperateWeatherDuration => TemperateWeather.Duration;
    public int HazardousWeatherStartCycleDay => TemperateWeatherDuration + 1;
    public int CycleLengthInDays => TemperateWeatherDuration + HazardousWeatherDuration;
}

public readonly record struct ModdableWeatherCycleWeather(string Id, int Duration);

public readonly record struct OnModdableWeatherCycleDecided(ModdableWeatherCycle WeatherCycle);

public class ModdableWeatherCycleSerializer : IValueSerializer<ModdableWeatherCycle>
{
    public static readonly ModdableWeatherCycleSerializer Instance = new();

    public Obsoletable<ModdableWeatherCycle> Deserialize(IValueLoader valueLoader)
    {
        var json = valueLoader.AsString();
        return JsonConvert.DeserializeObject<ModdableWeatherCycle>(json)!;
    }

    public void Serialize(ModdableWeatherCycle value, IValueSaver valueSaver)
    {
        valueSaver.AsString(JsonConvert.SerializeObject(value));
    }
}