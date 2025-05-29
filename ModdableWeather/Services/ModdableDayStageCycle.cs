
namespace ModdableWeather.Services;

public class ModdableDayStageCycle(
    IDayNightCycle dayNightCycle,
    ModdableWeatherService weatherService,
    ModdableHazardousWeatherService hazardousWeatherService,
    ISpecService specService
) : DayStageCycle(dayNightCycle, weatherService, hazardousWeatherService, specService),
    ILoadableSingleton, IUnloadableSingleton
{
    readonly ModdableWeatherService weatherService = weatherService;

    static ModdableDayStageCycle? instance;
    public static ModdableDayStageCycle Instance => instance.InstanceOrThrow();

    public new void Load()
    {
        instance = this;
        base.Load();
    }

    public void Unload()
    {
        instance = null;
    }

    public DayStageTransition NewTransition(DayStage currentDayStage, DayStage nextDayStage, float hoursToNextDayStage)
    {
        float t = 1f - Mathf.Clamp01(hoursToNextDayStage / _transitionLengthInHours);
        float transitionProgress = Mathf.SmoothStep(0f, 1f, t);

        var curr = weatherService.CurrentWeather.Id;
        var next = nextDayStage == DayStage.Sunrise ? weatherService.NextDayWeather.Id : curr;
        return new DayStageTransition(currentDayStage, curr, nextDayStage, next, transitionProgress);
    }
}
