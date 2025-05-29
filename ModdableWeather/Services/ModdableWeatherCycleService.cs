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
        var nextTemperate = generator.DecideTemperateWeatherForCycle(cycle + 1, history);

        ModdableWeatherUtils.Log(() => 
            $"Next temperate weather: {nextTemperate}");

        history.AddCycle(weather, nextTemperate);
    }

}
