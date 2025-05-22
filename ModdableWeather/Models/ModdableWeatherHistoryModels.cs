namespace ModdableWeather.Models;

public class ModdableWeatherCycle
{
    public int Cycle { get; init; }
    
    public ModdableWeatherCycleWeather TemperateWeather { get; init; }
    public ModdableWeatherCycleWeather HazardousWeather { get; init; }
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