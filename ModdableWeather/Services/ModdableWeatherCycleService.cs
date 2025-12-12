namespace ModdableWeather.Services;

public class ModdableWeatherCycleService(
    ModdableWeatherGenerator generator,
    ModdableWeatherHistoryProvider history
) : ICycleDuration
{

    public int DurationInDays => history.CurrentCycle.CycleLengthInDays;

    public void SetForCycle(int cycle)
    {
        var weather = generator.DecideForCycle(cycle, history);
        var nextCycle = generator.DecideNextCycleWeather(cycle, history);

        ModdableWeatherUtils.LogVerbose(() => 
            $"Next cycle weather: {nextCycle}");

        history.AddCycle(weather, nextCycle);
    }

}
