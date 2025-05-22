namespace ModdableWeather.Services;

public class ModdableWeatherService(
    ModdableWeatherHistoryProvider historyProvider,
    GameCycleService gameCycleService,
    EventBus eb
) : ILoadableSingleton, IUnloadableSingleton
{

    public int Cycle => gameCycleService.Cycle;
    public int CycleDay => gameCycleService.CycleDay;
    public float PartialCycleDay => gameCycleService.PartialCycleDay;

    public ModdableWeatherHistoryProvider HistoryProvider { get; } = historyProvider;

    public static ModdableWeatherService? Instance { get; private set; }

    public void Load()
    {
        Instance = this;

        eb.Register(this);
    }

    public void Unload()
    {
        Instance = null;
    }

}
