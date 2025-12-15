namespace ModdableWeathers.Historical;

public record WeatherCycleStage(int Index, bool IsBenign, string WeatherId, int Days);

public record WeatherCycle(int Cycle, ImmutableArray<WeatherCycleStage> Stages)
{

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static WeatherCycle Deserialize(string data) => JsonConvert.DeserializeObject<WeatherCycle>(data)!;

}

public record DetailedWeatherCycleStage(int Index, bool IsBenign, IModdableWeather Weather, int Days);
public record DetailedWeatherCycle(WeatherCycle WeatherCycle, ImmutableArray<DetailedWeatherCycleStage> Stages)
{
    public int Cycle { get; } = WeatherCycle.Cycle;
}