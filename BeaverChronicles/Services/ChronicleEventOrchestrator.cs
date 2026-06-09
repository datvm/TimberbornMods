namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventOrchestrator(
    ChronicleEventRegistry registry,
    ChronicleEventRecords history,
    ChronicleGameEventHandler gameEventHandler,
    IDayNightCycle dayNightCycle,
    ActiveChronicleEventService activeEventService,
    HelperCollection helperCollection
) : ILoadableSingleton, IPostLoadableSingleton, ITickableSingleton
{

    public IChronicleEvent? ActiveEvent { get; private set; }
    public bool HasActiveEvent => ActiveEvent is not null;

    public event EventHandler<IChronicleEvent>? BeforeEventTriggered;
    public event EventHandler<IChronicleEvent>? EventEnded;
    public event EventHandler<IChronicleEvent?> ActiveEventChanged = null!;

    public void Load()
    {
        ActiveEventChanged += OnActiveEventChanged;

        gameEventHandler.GameEventTriggered += OnGameEvent;
        TriggerSavedEvent();
    }

    void OnActiveEventChanged(object sender, IChronicleEvent? e)
    {
        activeEventService.SetActiveEvent(e);
    }

    public void PostLoad()
    {
        if (MetMinimumEventDay)
        {
            gameEventHandler.StartListening();
        }
    }

    public void ConcludeEvent(ChronicleEventContext context, bool? doNotRepeat = null)
    {
        ConcludeEvent(context.Event, context.Record, doNotRepeat);
    }

    void ConcludeEvent(IChronicleEvent e, EventHistoryRecord record, bool? doNotRepeat = null)
    {
        BeaverChroniclesUtils.Log($"Concluding event {e.Id}");

        history.EndEvent(record);
        EventEnded?.Invoke(this, e);

        if (ActiveEvent == e)
        {
            ActiveEvent = null;
            ActiveEventChanged?.Invoke(this, null);
        }

        doNotRepeat ??= (!e.CanRepeat);
        if (doNotRepeat.Value)
        {
            history.MarkFinished(e.Id);
        }
    }

    public void RequestNextEvent(string? id)
    {
        if (id is not null && !registry.Has(id))
        {
            throw new ArgumentException($"Event ID '{id}' not found in registry.", nameof(id));
        }

        history.NextEventIdRequested = id;
    }

    public void RequestNextEventDelay(float delayDays) => history.RequestNextEventDelay(delayDays);

    public bool HasCompletedEvent(string id) => history.HasFinished(id);

    void OnGameEvent(object sender, IEventTriggerParameters p)
    {
        var e = ChooseEvent(p);
        if (e is null) { return; }

        var record = history.StartEvent(e.Id, e is IMiniChronicleEvent);
        PerformTrigger(e, p, record);
    }

    IChronicleEvent? ChooseEvent(IEventTriggerParameters p)
    {
        if (!MetMinimumEventDay) // Should not happen but just in case
        {
            gameEventHandler.StopListening();
            return null;
        }

        var specific = p is IEventSpecificTriggerParameters specificParams ? specificParams.Event : null;
        if (HasActiveEvent && specific is not IMiniChronicleEvent)
        {
            return null;
        }

        var requestedNextEventId = history.NextEventIdRequested;
        if (requestedNextEventId is not null)
        {
            // If a specific event is being requested but it doesn't match the requested next event ID, skip.
            if (specific is not null && requestedNextEventId != specific.Id) { return null; }

            return ChooseRequestedEvent(p, requestedNextEventId);
        }

        if (specific is not null) // If a specific event is being triggered, try to trigger it directly (but only if it matches the requested next event ID if there is one)
        {
            return ChooseRequestedEvent(p, specific.Id);
        }

        var events = registry.EventsByTrigger[p.Source];
        if (events.Length == 0) { return null; }

        var totalWeight = 0;
        List<(IChronicleEvent, int)> weightedEvents = [];
        List<IChronicleEvent> maxWeightEvents = [];

        foreach (var e in events)
        {
            if (HasActiveEvent && e is not IMiniChronicleEvent) { continue; }

            if (history.HasFinished(e.Id)) { continue; }

            var weight = e.GetTriggerWeight(CreateContext(p));
            if (weight <= 0)
            {
                if (weight == -1)
                {
                    history.MarkFinished(e.Id);
                }

                continue;
            }

            if (weight == int.MaxValue)
            {
                maxWeightEvents.Add(e);
                continue;
            }

            if (maxWeightEvents.Count > 0) { continue; }

            totalWeight += weight;
            weightedEvents.Add((e, weight));
        }

        if (maxWeightEvents.Count > 0)
        {
            return maxWeightEvents.Count == 1
                ? maxWeightEvents[0]
                : maxWeightEvents[Random.Range(0, maxWeightEvents.Count)];
        }

        if (totalWeight == 0) { return null; }

        var randomValue = Random.Range(0, totalWeight);
        foreach (var (e, weight) in weightedEvents)
        {
            if (randomValue < weight)
            {
                return e;
            }

            randomValue -= weight;
        }

        return null; // Should never reach here
    }

    IChronicleEvent? ChooseRequestedEvent(IEventTriggerParameters p, string id)
    {
        if (history.HasFinished(id) || !registry.TryGet(id, out var e))
        {
            history.NextEventIdRequested = null;
            return null;
        }

        if (!e.TriggerSources.Contains(p.Source)) { return null; }
        if (HasActiveEvent && e is not IMiniChronicleEvent) { return null; }

        var weight = e.GetTriggerWeight(CreateContext(p));
        if (weight == -1)
        {
            history.NextEventIdRequested = null;
            history.MarkFinished(e.Id);
            return null;
        }

        return weight > 0 ? e : null;
    }

    void TriggerSavedEvent()
    {
        var activeId = history.ActiveEventId;
        if (activeId is null) { return; }

        if (registry.TryGet(activeId, out var e))
        {
            PerformTrigger(e, IEventTriggerParameters.GameLoad, history.ActiveRecord!);
        }
        else
        {
            Debug.LogWarning($"Active event ID '{activeId}' not found in registry. Clearing active event.");
            history.EndEvent();
        }
    }

    void PerformTrigger(IChronicleEvent e, IEventTriggerParameters parameters, EventHistoryRecord record)
    {
        var isMiniEvent = e is IMiniChronicleEvent;
        if (!isMiniEvent)
        {
            if (HasActiveEvent)
            {
                throw new InvalidOperationException($"Only mini event can be triggered when there is already an active event. Active event: {ActiveEvent}, attempted to trigger: {e}");
            }
        }
        else
        {
            if (parameters.Source == EventTriggerSource.GameLoad)
            {
                throw new InvalidOperationException("Mini events cannot be triggered from game load.");
            }
        }
        
        BeaverChroniclesUtils.Log($"Triggering event {e.Id}");
        TimberUiUtils.LogVerbose(() => "- Parameters: " + parameters);

        // Clear the requested next event parameters since we're triggering an event (whether it's the requested one or not)
        history.NextEventIdRequested = null;
        history.RequestNextEventDelay(0);

        BeforeEventTriggered?.Invoke(this, e);
        if (!isMiniEvent)
        {
            ActiveEvent = e;
            ActiveEventChanged?.Invoke(this, e);
        }

        e.Trigger(CreateContext(parameters, e, record));
    }

    public void Tick()
    {
        if (gameEventHandler.Listening) { return; }

        if (MetMinimumEventDay)
        {
            gameEventHandler.StartListening();
        }
    }

    public bool MetMinimumEventDay => dayNightCycle.PartialDayNumber >= history.NextEventMinimumDay;

    ChronicleTriggerContext CreateContext(IEventTriggerParameters p) => new(p, history, helperCollection);

    ChronicleEventContext CreateContext(IEventTriggerParameters p, IChronicleEvent ev, EventHistoryRecord record)
    {
        var occurrence = history.GetOccurrence(record);
        var context = new ChronicleEventContext(ev, record, occurrence, p, history, helperCollection, this);
        context.Initialize();
        helperCollection.Flags.MarkEvent(ev.Id, occurrence);
        return context;
    }
}
