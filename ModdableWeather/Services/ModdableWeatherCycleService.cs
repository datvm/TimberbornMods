namespace ModdableWeather.Services;

public class ModdableWeatherCycleService(
    ModdableWeatherGenerator generator,
    ModdableWeatherHistoryProvider moddableWeatherHistoryProvider
) : ICycleDuration
{

    public int DurationInDays => moddableWeatherHistoryProvider.CurrentCycle.CycleLengthInDays;

    public void SetForCycle(int cycle)
    {
        generator.DecideForCycle(cycle);
    }

}
