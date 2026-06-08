namespace BeaverChronicles.Services;

public class ChronicleEventContext(
    IEventTriggerParameters parameters,
    ChronicleEventHistoryService history,
    ChronicleEventFlagHelper flagHelper,
    ChronicleEventService service
)
{
    public IEventTriggerParameters Parameters { get; } = parameters;
    public ChronicleEventHistoryService History { get; } = history;
    public ChronicleEventFlagHelper FlagHelper { get; } = flagHelper;
    public EventHistoryRecord? ActiveRecord => History.ActiveRecord;

    public bool HasCompletedEvent(string id) => History.HasFinished(id);
    public void RequestNextEvent(string? id) => service.RequestNextEvent(id);
    public void RequestNextEventDelay(float delayDays) => History.RequestNextEventDelay(delayDays);
    public void ConcludeEvent(bool? doNotRepeat = null) => service.ConcludeEvent(doNotRepeat);
}
