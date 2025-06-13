namespace ModdableWeather.Models;

public record ModdableWeatherCycle(int Cycle, ModdableWeatherCycleWeather TemperateWeather, ModdableWeatherCycleWeather HazardousWeather)
{
    public int HazardousWeatherDuration => HazardousWeather.Duration;
    public int TemperateWeatherDuration => TemperateWeather.Duration;
    public int HazardousWeatherStartCycleDay => TemperateWeatherDuration + 1;
    public int CycleLengthInDays => TemperateWeatherDuration + HazardousWeatherDuration;
}

public readonly record struct ModdableWeatherCycleDetails(ModdableWeatherCycle Cycle, IModdedTemperateWeather TemperateWeather, IModdedHazardousWeather HazardousWeather);

public readonly record struct ModdableWeatherCycleWeather(string Id, int Duration);

public readonly record struct OnModdableWeatherCycleDecided(ModdableWeatherCycleDetails WeatherCycle);

public readonly record struct OnModdableWeatherChangedMidCycle(ModdableWeatherCycleDetails WeatherCycle);

[method: JsonConstructor]
public record ModdableWeatherNextCycleWeather(bool SingleMode, bool IsTemperate, string TemperateWeatherId)
{
    [JsonIgnore]
    public IModdedTemperateWeather? TemperateWeather { get; set; }

    public ModdableWeatherNextCycleWeather(bool SingleMode, bool IsTemperate, IModdedTemperateWeather TemperateWeather)
        : this(SingleMode, IsTemperate, TemperateWeather.Id)
    {
        this.TemperateWeather = TemperateWeather;
    }

}

public class ModdableWeatherJsonSerializer<T> : IValueSerializer<T>
{
    static readonly Lazy<ModdableWeatherJsonSerializer<T>> instance = new(() => new());
    public static ModdableWeatherJsonSerializer<T> Instance => instance.Value;

    public Obsoletable<T> Deserialize(IValueLoader valueLoader)
    {
        var json = valueLoader.AsString();
        return JsonConvert.DeserializeObject<T>(json)!;
    }

    public void Serialize(T value, IValueSaver valueSaver)
    {
        valueSaver.AsString(JsonConvert.SerializeObject(value));
    }

}