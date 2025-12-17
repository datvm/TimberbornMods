
namespace ModdableWeathers.Historical;

public readonly record struct WeatherCycleStageDefinition(int Index, int SkipChance, int BenignChance, int LengthMultiplier);

public class WeatherCycleDecision
{
    public int Cycle { get; init; }
    public ImmutableArray<WeatherCycleStageDecision> Stages { get; init; }
}

public class WeatherCycleStageDecision
{
    public int Cycle { get; init; }
    public int StageIndex { get; init; }
    public bool IsBenign { get; set; }
    public IModdableWeather? Weather { get; set; }
    public int Days { get; set; }
    public int DaysMultiplier { get; set; }
    public List<IModdableWeatherModifier> Modifiers { get; init; } = [];

    public int GetDaysEffective()
    {
        if (Days <= 0) { return 0; }

        var days = Mathf.RoundToInt(Days * (DaysMultiplier / 100f));
        return Mathf.Max(days, 1);
    }

}