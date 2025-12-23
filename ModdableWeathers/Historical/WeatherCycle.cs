namespace ModdableWeathers.Historical;

public record WeatherCycleStage(
    int Index,
    bool IsBenign,
    string WeatherId,
    ImmutableArray<string> WeatherModifierIds,
    int Days
)
{
    public override string ToString() => $"Stage {Index}: {WeatherId} for {Days} days";
}

public record WeatherCycle(int Cycle, ImmutableArray<WeatherCycleStage> Stages)
{
    public static readonly WeatherCycle Empty = new(0, []);

    public string Serialize() => JsonConvert.SerializeObject(this);
    public static WeatherCycle Deserialize(string data) => JsonConvert.DeserializeObject<WeatherCycle>(data)!;

    public int TotalDurationInDays => Stages.Sum(s => s.Days);

    public override string ToString()
        => $"Cycle {Cycle}, Days: {TotalDurationInDays}, Stages: {string.Join("; ", Stages)}";

}

