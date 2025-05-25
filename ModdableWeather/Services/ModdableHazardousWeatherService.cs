namespace ModdableWeather.Services;

public class ModdableHazardousWeatherService(
    EventBus eventBus,
    ISingletonLoader singletonLoader,
    MapEditorMode mapEditorMode,
    DroughtWeather droughtWeather,
    BadtideWeather badtideWeather,
    HazardousWeatherRandomizer hazardousWeatherRandomizer,
    HazardousWeatherHistory hazardousWeatherHistory,
    ModdableWeatherHistoryProvider history,
    ModdableWeatherRegistry registry
) : HazardousWeatherService(eventBus, singletonLoader, mapEditorMode, droughtWeather, badtideWeather, hazardousWeatherRandomizer, hazardousWeatherHistory),
    ILoadableSingleton, IUnloadableSingleton
{
    static ModdableHazardousWeatherService? instance;
    public static ModdableHazardousWeatherService Instance => instance.InstanceOrThrow();

    public new IHazardousWeather CurrentCycleHazardousWeather => registry.GetHazardousWeather(
        history.CurrentCycle.HazardousWeather.Id);

    public new int HazardousWeatherDuration => history.CurrentCycle.HazardousWeatherDuration;
    public new int DurationInDays => history.CurrentCycle.HazardousWeatherDuration;

    public new void Load()
    {
        instance = this;
    }

    public void Unload()
    {
        instance = null;
    }
}
