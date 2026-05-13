namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventService(
    ChronicleEventRegistry registry,
    ChronicleEventHistoryService history,
    ChronicleGameEventHandler gameEventHandler,
    IDayNightCycle dayNightCycle,
    ActiveChronicleEventService activeEventService
) : ILoadableSingleton, IPostLoadableSingleton, ITickableSingleton
{
    public readonly ChronicleEventRegistry Registry = registry;
    public readonly ChronicleEventHistoryService History = history;

    public IChronicleEvent? ActiveEvent { get; private set; }
    public EventHistoryRecord? ActiveRecord => History.ActiveRecord;
    public bool HasActiveEvent => ActiveEvent is not null;

    public event EventHandler<IChronicleEvent>? BeforeEventTriggered;
    public event EventHandler<IChronicleEvent>? OnEventEnded;
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
        if (!HasActiveEvent)
        {
            gameEventHandler.StartListening();
        }
    }

    public void ConcludeEvent(bool? doNotRepeat = null)
    {
        if (!HasActiveEvent)
        {
            throw new InvalidOperationException("No active event to conclude.");
        }

        var e = ActiveEvent!;
        BeaverChroniclesUtils.Log($"Concluding event {e.Id}");

        History.EndEvent();

        OnEventEnded?.Invoke(this, e);

        doNotRepeat ??= (!e.CanRepeat);
        if (doNotRepeat == true)
        {
            History.FinishedEventIds.Add(e.Id);
        }

        ActiveEvent = null;
        ActiveEventChanged?.Invoke(this, null);

        if (MetMinimumEventDay)
        {
            gameEventHandler.StartListening();
        }
    }

    public void RequestNextEvent(string? id)
    {
        if (id is not null && !Registry.Has(id))
        {
            throw new ArgumentException($"Event ID '{id}' not found in registry.", nameof(id));
        }

        History.NextEventIdRequested = id;
    }

    public void RequestNextEventDelay(float delayDays) => History.RequestNextEventDelay(delayDays);

    public bool HasCompletedEvent(string id) => History.FinishedEventIds.Contains(id);

    void OnGameEvent(object sender, IEventTriggerParameters p)
    {
        var e = ChooseEvent(p);
        if (e is null) { return; }

        PerformTrigger(e, p);
    }

    IChronicleEvent? ChooseEvent(IEventTriggerParameters p)
    {
        if (!MetMinimumEventDay) // Should not happen but just in case
        {
            gameEventHandler.StopListening();
            return null;
        }

        var requestedNextEventId = History.NextEventIdRequested;
        if (requestedNextEventId is not null)
        {
            return ChooseRequestedEvent(p, requestedNextEventId);
        }

        var events = Registry.EventsByTrigger[p.Source];
        if (events.Length == 0) { return null; }

        var finished = History.FinishedEventIds;
        var totalWeight = 0;
        List<(IChronicleEvent, int)> weightedEvents = [];
        List<IChronicleEvent> maxWeightEvents = [];

        foreach (var e in events)
        {
            if (finished.Contains(e.Id)) { continue; }

            var weight = e.GetTriggerWeight(p, this);
            if (weight <= 0)
            {
                if (weight == -1)
                {
                    finished.Add(e.Id);
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
        var finished = History.FinishedEventIds;

        if (finished.Contains(id) || !Registry.TryGet(id, out var e))
        {
            History.NextEventIdRequested = null;
            return null;
        }

        if (!e.TriggerSources.Contains(p.Source)) { return null; }

        var weight = e.GetTriggerWeight(p, this);
        if (weight == -1)
        {
            History.NextEventIdRequested = null;
            finished.Add(e.Id);
            return null;
        }

        return weight > 0 ? e : null;
    }

    void TriggerSavedEvent()
    {
        var activeId = History.ActiveEventId;
        if (activeId is null) { return; }

        if (Registry.TryGet(activeId, out var e))
        {
            PerformTrigger(e, IEventTriggerParameters.GameLoad);
        }
        else
        {
            Debug.LogWarning($"Active event ID '{activeId}' not found in registry. Clearing active event.");
            History.EndEvent();
        }
    }

    void PerformTrigger(IChronicleEvent e, IEventTriggerParameters parameters)
    {
        gameEventHandler.StopListening();

        if (parameters.Source != EventTriggerSource.GameLoad)
        {
            History.StartEvent(e.Id);
        }

        BeaverChroniclesUtils.Log($"Triggering event {e.Id}");
        TimberUiUtils.LogVerbose(() => "- Parameters: " + parameters);

        // Clear the requested next event parameters since we're triggering an event (whether it's the requested one or not)
        History.NextEventIdRequested = null;
        History.RequestNextEventDelay(0);

        ActiveEvent = e;
        BeforeEventTriggered?.Invoke(this, e);
        ActiveEventChanged?.Invoke(this, e);
        e.Trigger(parameters, this);
    }

    public void Tick()
    {
        if (gameEventHandler.Listening || HasActiveEvent) { return; }

        if (MetMinimumEventDay)
        {
            gameEventHandler.StartListening();
        }
    }

    public bool MetMinimumEventDay => dayNightCycle.PartialDayNumber >= History.NextEventMinimumDay;

}
