namespace BetterWeatherStation.Services;

public interface ICurrentWeatherStatus
{
    int Cycle { get; }
    int CycleDay { get; }
    CompatWeatherCycleStage CurrentStage { get; }
    CompatNextWeatherCycleStage NextStage { get; }
    float HoursToNextStage { get; }
}

class CurrentWeatherStatus : ICurrentWeatherStatus
{
    public int Cycle { get; set; } = -1;
    public int CycleDay { get; set; } = -1;

    public CompatWeatherCycleStage CurrentStage { get; set; } = null!;
    public CompatNextWeatherCycleStage NextStage { get; set; } = null!;
    public int NextStageCycleDay { get; set; } = -1;
    public float HoursToNextStage { get; set; } = -1;

    public CompatWeatherCycle WeatherCycleInfo = null!;

    public int GetCurrentStageIndex(int cycleDay)
    {
        var days = 1;
        for (int i = 0; i < WeatherCycleInfo.Stages.Length; i++)
        {
            days += WeatherCycleInfo.Stages[i].Length;

            if (cycleDay < days)
            {
                return i;
            }
        }

        return WeatherCycleInfo.Stages.Length;
    }

}
