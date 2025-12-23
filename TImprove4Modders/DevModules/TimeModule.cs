namespace TImprove4Modders.DevModules;

public class TimeModule(
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService,
    WeatherService weatherService,
    ISpecService specService,
    DialogService diag
) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Jump to the next day", JumpToNextDay))
            .AddMethod(DevMethod.Create("Jump to before weather warning day", JumpToBeforeWarningDay))
            .AddMethod(DevMethod.Create("Jump to before hazardous weather", JumpToBeforeHazardousWeather))
            .AddMethod(DevMethod.Create("Jump to day", JumpToDay))
            .Build();
    }

    async void JumpToDay()
    {
        var dayInput = await diag.PromptAsync("Enter day:", "1");
        if (string.IsNullOrEmpty(dayInput) || !int.TryParse(dayInput, out var targetDay) || targetDay < 1)
        {
            return;
        }

        var startCycle = gameCycleService.Cycle;
        while (gameCycleService.CycleDay < targetDay)
        {
            JumpToNextDay();
            if (gameCycleService.Cycle != startCycle) { break; }
        }
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
        var hazardStart = weatherService.HazardousWeatherStartCycleDay;
        var days = specService.GetSingleSpec<HazardousWeatherUISpec>();

        return hazardStart - days.ApproachingNotificationDays - 1;
    }

}
