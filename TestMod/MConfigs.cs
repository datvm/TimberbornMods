using ModdableTimberborn.CompatWeatherService;

namespace TestMod;

public class MConfig : BaseModdableTimberbornAttributeConfiguration
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.All;
}

[MultiBind(typeof(IDevModule))]
class Test(CompatWeatherService service) : IDevModule
{
    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Test weather compat", TestWeather))
            .Build();
    }

    void TestWeather()
    {
        var p = service.Provider;

        Debug.Log(JsonConvert.SerializeObject((object?[])[
            p.GetType().Name,
            string.Join(", ", p.WeatherTypes),
            p.ApproachingNotificationDays,
            p.CurrentCycleHazardousId,
            p.IsHazardous,
            p.IsNextDayHazardous,
            p.GetCurrentCycle(),
            p.GetCurrentCycleStage(),
            p.GetCurrentCycleBenignStage(),
            p.GetCurrentCycleHazardousStage(),            
            p.GetNextCycleStage(),
            p.GetWarningStatus(),
        ], Formatting.Indented));
    }

}