namespace TImprove4Modders.DevModules;

public class TimeModule(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService,
    HazardousWeatherApproachingTimer hazardousWeatherApproachingTimer,
    WeatherService weatherService
) : IDevModule
{
    // See if ModdableWeather is available
    readonly PropertyInfo? daysUntilWarningProp = hazardousWeatherApproachingTimer.GetType()
        .GetProperty("DaysUntilWarning");

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Jump to the next day", JumpToNextDay))
            .AddMethod(DevMethod.Create("Jump to before weather warning day", JumpToBeforeWarningDay))
            .AddMethod(DevMethod.Create("Jump to before hazardous weather", JumpToBeforeHazardousWeather))
            .Build();
    }

    void JumpToNextDay()
    {
        dayNightCycle.SetTimeToNextDay();
    }

    void JumpToBeforeWarningDay()
    {
        var days = GetDaysUntilBeforeWarning();
        SkipDays(days);
    }

    void JumpToBeforeHazardousWeather()
    {
        var days = GetDaysUntilBeforeHazardous();
        SkipDays(days);
    }

    void SkipDays(int count)
    {
        for (int i = 0; i < count; i++)
        {
            dayNightCycle.SetTimeToNextDay();
        }
    }

    int GetDaysUntilBeforeHazardous() 
        => weatherService.HazardousWeatherStartCycleDay - gameCycleService.CycleDay - 1;

    int GetDaysUntilBeforeWarning()
    {
        if (daysUntilWarningProp is null)
        {
            return (int)hazardousWeatherApproachingTimer.DaysToHazardousWeather - HazardousWeatherApproachingTimer.ApproachingNotificationDays - 1;
        }
        else
        {
            return (int)daysUntilWarningProp.GetValue(hazardousWeatherApproachingTimer) - 1;
        }
    }

}
