
namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleGameEventHandler(
    EventBus eb,
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService,
    ITimeTriggerFactory timeTriggerFactory,
    IWeatherIdService weatherIdService
) : ITickableSingleton, ISaveableSingleton, ILoadableSingleton
{
    const float NewDayDelay = .5f / 24f; // Delay half an hour so it's not conflict with other events.

    static readonly SingletonKey SaveKey = new(nameof(ChronicleGameEventHandler));
    static readonly PropertyKey<int> HourKey = new("Hour");
    static readonly PropertyKey<int> DayKey = new("Day");
    static readonly PropertyKey<int> CycleKey = new("Cycle");
    static readonly PropertyKey<string> WeatherKey = new("Weather");

    public bool Listening { get; private set; }
    int currHour, currDay, currCycle;
    string currWeather = "";

    public event EventHandler<IEventTriggerParameters> GameEventTriggered = null!;

    public void Load()
    {
        LoadSavedData();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(HourKey, currHour);
        s.Set(DayKey, currDay);
        s.Set(CycleKey, currCycle);
        s.Set(WeatherKey, currWeather);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        currHour = s.Get(HourKey);
        currDay = s.Get(DayKey);
        currCycle = s.Get(CycleKey);
        currWeather = s.Get(WeatherKey);
    }

    public void StartListening()
    {
        if (Listening) { return; }

        Listening = true;
        eb.Register(this);
    }

    public void StopListening()
    {
        if (!Listening) { return; }

        Listening = false;
        eb.UnregisterNow(this);
    }

    public void Tick()
    {
        if (!Listening) { return; }

        ProcessDayTime();
    }

    [OnEvent]
    public void OnCharacterCreated(CharacterCreatedEvent e)
        => Trigger(Create(EventTriggerSource.CharacterCreated, Create(e.Character)));

    [OnEvent]
    public void OnBeaverGrownUp(BeaverGrowTracker.BeaverGrownUpEvent e)
        => Trigger(Create(EventTriggerSource.BeaverGrownUp, new BeaverGrownUpParameters(e.Character, e.Child)));

    [OnEvent]
    public void OnCharacterDeath(CharacterKilledEvent e)
        => Trigger(Create(EventTriggerSource.CharacterDeath, Create(e.Character)));

    [OnEvent]
    public void OnToolUnlocked(ToolUnlockedEvent e)
    {
        var t = e.Tool;
        Trigger(Create(EventTriggerSource.ToolUnlocked, t));

        if (t is not BlockObjectTool bot) { return; }
        var template = bot.Template;
        Trigger(Create(EventTriggerSource.BuildingUnlocked, Create(template, bot)));
    }

    [OnEvent]
    public void OnEntityInitialized(EntityInitializedEvent e)
    {
        var pbos = e.Entity.GetComponent<PlaceableBlockObjectSpec>();
        if (pbos is null) { return; }

        Trigger(Create(EventTriggerSource.BuildingPlaced, Create(pbos, null)));
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent e)
    {
        var pbos = e.BlockObject.GetComponent<PlaceableBlockObjectSpec>();
        if (pbos is null) { return; }

        Trigger(Create(EventTriggerSource.BuildingFinished, Create(pbos, null)));
    }

    [OnEvent]
    public void OnWellbeingHighscore(NewWellbeingHighscoreEvent e)
    {
        Trigger(Create(EventTriggerSource.NewWellbeingHighscore, e.WellbeingHighscore));
    }

    void Trigger(IEventTriggerParameters p)
    {
        if (!Listening) { return; }
        GameEventTriggered(this, p);
    }

    void ProcessDayTime()
    {
        var partialDay = dayNightCycle.PartialDayNumber;
        var day = (int)partialDay;
        var hours = (partialDay - day) * 24f;
        var hour = (int)hours;

        if (hour == currHour) { return; }

        var cycle = gameCycleService.Cycle;
        var weatherId = weatherIdService.GetWeatherId();
        var parameters = CreateParameters();

        Trigger(Create(EventTriggerSource.NewHour, parameters));
        currHour = hour;

        if (day == currDay) { return; }        
        timeTriggerFactory.CreateAndStart(
            () => Trigger(Create(EventTriggerSource.NewDay, CreateParameters())),  // Recreate parameters to get updated hour.
            NewDayDelay);
        currDay = day;

        if (weatherId != currWeather)
        {
            Trigger(Create(EventTriggerSource.NewWeather, parameters));
            currWeather = weatherId;
        }

        if (cycle == currCycle) { return; }
        Trigger(Create(EventTriggerSource.NewCycle, parameters));
        currCycle = cycle;

        DayTimeParameters CreateParameters() => new(day, cycle, gameCycleService.CycleDay, partialDay, hours, weatherId);
    }

    static IEventTriggerParameters Create<T>(EventTriggerSource source, T data)
        => new EventTriggerParameter<T>(source, data);

    static CharacterParameters Create(Character c)
    {
        var isBeaver = c.HasComponent<BeaverSpec>();
        var isAdult = !c.HasComponent<ChildSpec>();
        return new(c, isBeaver, isAdult);
    }

    static BuildingParameters Create(PlaceableBlockObjectSpec pbos, BlockObjectTool? tool)
        => new(pbos.GetTemplateName(), pbos, tool);

}
