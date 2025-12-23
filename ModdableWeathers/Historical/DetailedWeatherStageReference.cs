namespace ModdableWeathers.Historical;

public readonly record struct DetailedWeatherStageReference(
    DetailedWeatherCycle Cycle,
    DetailedWeatherCycleStage Stage
)
{
    public int CycleIndex => Cycle.Cycle;
    public int StageIndex => Stage.Index;
    public IModdableWeather Weather => Stage.Weather;
    public ImmutableArray<IModdableWeatherModifier> WeatherModifiers => Stage.WeatherModifiers;

    public int CalculateStartDay()
    {
        var startDay = 1;

        for (int i = 0; i < Stage.Index; i++)
        {
            startDay += Cycle.Stages[i].Days;
        }

        return startDay;
    }

    public int CalculateEndDay() => CalculateStartDay() + Stage.Days - 1;
    public string ListEffects(ILoc t) => Stage.ListEffects(t);

}
