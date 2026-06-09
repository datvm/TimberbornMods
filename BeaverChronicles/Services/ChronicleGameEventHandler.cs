namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleGameEventHandler(
    EventBus eb,
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    GameCycleService gameCycleService,
    ITimeTriggerFactory timeTriggerFactory,
    DefaultEntityTracker<Beaver> beavers,
    DefaultEntityTracker<Bot> bots,
    CompatWeatherService compatWeatherService
) : ITickableSingleton, ISaveableSingleton, ILoadableSingleton
{
    const float NewDayDelay = .5f / 24f; // Delay half an hour so it's not conflict with other events.

    public const string InjuryId = "Injury";
    public const string ContaminationId = "BadwaterContamination";

    static readonly SingletonKey SaveKey = new(nameof(ChronicleGameEventHandler));
    static readonly PropertyKey<int> HourKey = new("Hour");
    static readonly PropertyKey<int> DayKey = new("Day");
    static readonly PropertyKey<int> CycleKey = new("Cycle");
    static readonly PropertyKey<string> WeatherKey = new("Weather");
    static readonly PropertyKey<string> EventCountKey = new("EventCount");

    public bool Listening { get; private set; }
    int currHour, currDay, currCycle;
    string currWeather = "";

    readonly Dictionary<EventTriggerSource, int> triggerCount = [];
    public int GetTriggerCount(EventTriggerSource source) => triggerCount.TryGetValue(source, out var c) ? c : 0;
    int GetAndIncreaseTriggerCount(EventTriggerSource source)
    {
        var c = GetTriggerCount(source);
        triggerCount[source] = c + 1;
        return c;
    }

    public event EventHandler<IEventTriggerParameters> GameEventTriggered = null!;

    public void Load()
    {
        LoadSavedData();

        beavers.OnEntityRegistered += OnCharacterRegistered;
        bots.OnEntityRegistered += OnCharacterRegistered;
    }

    void OnCharacterRegistered(BaseComponent c)
    {
        var needMan = c.GetComponent<NeedManager>();
        var character = c.GetComponent<Character>();
        needMan.NeedChangedActiveState += (_, e) => OnCharacterNeedChanged(character, e);
    }

    void OnCharacterNeedChanged(Character character, NeedChangedActiveStateEventArgs e)
    {
        var c = Create(character);
        var active = e.IsActive;
        var spec = e.NeedSpec;
        var p = new NeedChangedParameters(spec, active, c);

        Trigger(Create(active ? EventTriggerSource.NeedActivated : EventTriggerSource.NeedDeactivated, p));
        switch (spec.Id)
        {
            case InjuryId:
                Trigger(Create(active ? EventTriggerSource.Injured : EventTriggerSource.InjuryCured, c));
                break;
            case ContaminationId:
                Trigger(Create(active ? EventTriggerSource.Contaminated : EventTriggerSource.Decontaminated, c));
                break;
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(HourKey, currHour);
        s.Set(DayKey, currDay);
        s.Set(CycleKey, currCycle);
        s.Set(WeatherKey, currWeather);
        s.Set(EventCountKey, JsonConvert.SerializeObject(triggerCount));
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        currHour = s.Get(HourKey);
        currDay = s.Get(DayKey);
        currCycle = s.Get(CycleKey);
        currWeather = s.Get(WeatherKey);

        if (s.Has(EventCountKey))
        {
            var list = JsonConvert.DeserializeObject<Dictionary<EventTriggerSource, int>>(s.Get(EventCountKey)) ?? [];
            triggerCount.Clear();

            foreach (var (k, v) in list)
            {
                triggerCount[k] = v;
            }
        }
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
        eb.Unregister(this);
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
        if (CreateBuilding(e.Entity) is { } p)
        {
            Trigger(Create(EventTriggerSource.BuildingPlaced, p));
        }
    }

    [OnEvent]
    public void OnEnteredFinishedState(EnteredFinishedStateEvent e)
    {
        if (CreateBuilding(e.BlockObject) is { } p)
        {
            Trigger(Create(EventTriggerSource.BuildingPlaced, p));
        }
    }

    [OnEvent]
    public void OnWellbeingHighscore(NewWellbeingHighscoreEvent e)
    {
        Trigger(Create(EventTriggerSource.NewWellbeingHighscore, e.WellbeingHighscore));
    }

    [OnEvent]
    public void OnCustomEvent(OnCustomChronicleEvent e)
    {
        Trigger(Create(EventTriggerSource.Custom, new CustomEventParameters(e.Name, e.Data)));
    }

    [OnEvent]
    public void OnCharacterEnteredArea(OnCharacterEnteredAreaEvent e)
    {
        Trigger(Create(EventTriggerSource.CharacterEnteredArea,
            new CharacterInAreaParameters(Create(e.Character.GetComponent<Character>())),
            e.Event));
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
        var weatherId = compatWeatherService.Provider.GetCurrentCycleStage().WeatherId;
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

        var weatherWarning = compatWeatherService.Provider.GetWarningStatus();
        if (weatherWarning.Stage == CompatWeatherWarningStage.ShowedToday)
        {
            Trigger(Create(EventTriggerSource.WeatherWarning, new WeatherWarningParameters(weatherWarning.NextWeatherId!, parameters)));
        }

        if (cycle == currCycle) { return; }
        Trigger(Create(EventTriggerSource.NewCycle, parameters));
        currCycle = cycle;

        DayTimeParameters CreateParameters() => new(day, cycle, gameCycleService.CycleDay, partialDay, hours, weatherId);
    }

    IEventTriggerParameters Create<T>(EventTriggerSource source, T data, IChronicleEvent? ev = null)
        => ev is null
        ? new EventTriggerParameter<T>(source, GetAndIncreaseTriggerCount(source), data)
        : new EventSpecificTriggerParameter<T>(source, GetAndIncreaseTriggerCount(source), data, ev);

    static CharacterParameters Create(Character c)
    {
        var isBeaver = c.HasComponent<BeaverSpec>();
        var isAdult = !c.HasComponent<ChildSpec>();
        return new(c, isBeaver, isAdult);
    }

    static BuildingParameters Create(PlaceableBlockObjectSpec pbos, BlockObjectTool? tool)
        => new(pbos.GetTemplateName(), pbos, tool);

    static BuildingInstanceParameters? CreateBuilding(BaseComponent comp)
    {
        var pbos = comp.GetComponent<PlaceableBlockObjectSpec>();
        if (pbos is null) { return null; }

        var bo = comp is BlockObject cbo ? cbo : comp.GetComponent<BlockObject>();
        if (!bo) { return null; }

        return new(pbos.GetTemplateName(), pbos, null, bo);
    }
        
}
