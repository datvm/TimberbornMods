namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventService(
    ChronicleEventRegistry registry,
    ChronicleEventHistoryService history,
    ChronicleGameEventHandler gameEventHandler,
    IDayNightCycle dayNightCycle
) : ILoadableSingleton, IPostLoadableSingleton, ITickableSingleton
{
    public readonly ChronicleEventRegistry Registry = registry;
    public readonly ChronicleEventHistoryService History = history;

    public IChronicleEvent? ActiveEvent { get; private set; }
    public bool HasActiveEvent => ActiveEvent is not null;

    public event EventHandler<IChronicleEvent>? BeforeEventTriggered;
    public event EventHandler<IChronicleEvent>? OnEventEnded;

    public void Load()
    {
        gameEventHandler.GameEventTriggered += OnGameEvent;
        TriggerSavedEvent();
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

        if (MetMinimumEventDay)
        {
            gameEventHandler.StartListening();
        }
    }

    public void RequestNextEvent(string? id, float? days = null)
    {
        if (id is not null && !Registry.Has(id))
        {
            throw new ArgumentException($"Event ID '{id}' not found in registry.", nameof(id));
        }

        History.NextEventIdRequested = id;
        if (days.HasValue)
        {
            History.RequestNextEventDelay(days.Value);
        }
    }

    void OnGameEvent(object sender, IEventTriggerParameters p)
    {
        var e = ChooseEvent(p);
        if (e is null) { return; }

        PerformTrigger(e, p);
    }

    IChronicleEvent? ChooseEvent(IEventTriggerParameters p)
    {
        TimberUiUtils.LogDev($"Choosing event for trigger {p}");

        var events = Registry.EventsByTrigger[p.Source];
        if (events.Length == 0) { return null; }

        if (!MetMinimumEventDay) // Should not happen but just in case
        {
            gameEventHandler.StopListening();
            return null;
        }

        var finished = History.FinishedEventIds;

        var requestedNextEventId = History.NextEventIdRequested;
        if (requestedNextEventId is not null
            && (!Registry.Has(requestedNextEventId) || finished.Contains(requestedNextEventId)))
        {
            requestedNextEventId = History.NextEventIdRequested = null;
        }

        var totalWeight = 0;
        List<(IChronicleEvent, int)> weightedEvents = [];
        List<IChronicleEvent> maxWeightEvents = [];

        foreach (var e in events)
        {
            if (finished.Contains(e.Id)) { continue; }
            if (requestedNextEventId is not null && e.Id != requestedNextEventId) { continue; }

            var weight = e.GetTriggerWeight(p);
            if (weight <= 0)
            {
                if (weight == -1)
                {
                    finished.Add(e.Id);
                }

                continue;
            }

            if (requestedNextEventId is not null)
            {
                maxWeightEvents.Add(e);
                break;
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

        ActiveEvent = e;
        BeforeEventTriggered?.Invoke(this, e);
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
