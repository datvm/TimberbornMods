namespace ModdableWeathers.Historical;

public record DetailedWeatherCycle(WeatherCycle WeatherCycle, ImmutableArray<DetailedWeatherCycleStage> Stages)
{
    public int Cycle { get; } = WeatherCycle.Cycle;
    public int TotalDurationInDays { get; } = WeatherCycle.TotalDurationInDays;

    public DetailedWeatherStageReference GetStage(int stageIndex) => new(this, Stages[stageIndex]);
}

public record DetailedWeatherCycleStage(
    int Index,
    bool IsBenign,
    IModdableWeather Weather,
    ImmutableArray<IModdableWeatherModifier> WeatherModifiers,
    int Days
)
{
    public string ListEffects(ILoc t) => t.T("LV.MW.EffNotf",
        string.Join(", ", WeatherModifiers.Select(m => m.Spec.Name.Value)));
}
