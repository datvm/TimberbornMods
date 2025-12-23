namespace ModdableWeathers.Cycles;

public class CycleDurationService(
    WeatherHistoryRegistry history,
    WeatherGenerator weatherGenerator
) : ICycleDuration
{

    public int DurationInDays { get; private set; }

    public void SetForCycle(int cycle)
    {
        weatherGenerator.EnsureWeatherGenerated(cycle);
        
        var info = history[cycle];
        DurationInDays = Math.Max(1, info.TotalDurationInDays);

        ModdableWeathersUtils.LogVerbose(() => $"Starting cycle {cycle} with duration {DurationInDays} days.");
    }

}
