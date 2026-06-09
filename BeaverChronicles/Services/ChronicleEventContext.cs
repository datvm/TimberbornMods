namespace BeaverChronicles.Services;

public class ChronicleTriggerContext(
    IEventTriggerParameters parameters,
    ChronicleEventRecords history,
    HelperCollection helperCollection
)
{
    public IEventTriggerParameters Parameters => parameters;
    public ChronicleEventRecords History => history;
    public HelperCollection Helpers => helperCollection;
    public bool HasCompletedEvent(string id) => History.HasFinished(id);
}

public class ChronicleEventContext(
    IChronicleEvent ev,
    EventHistoryRecord record,
    int occurrence,
    IEventTriggerParameters parameters,
    ChronicleEventRecords history,
    HelperCollection helperCollection,
    ChronicleEventOrchestrator service
) : ChronicleTriggerContext(parameters, history, helperCollection)
{
    const string OccurrenceParameterKey = "Occurrence";

    public IChronicleEvent Event => ev;
    public EventHistoryRecord Record => record;
    public int Occurrence => occurrence;

    public bool IsMiniEvent { get; } = ev is IMiniChronicleEvent;

    public void Initialize()
    {
        record.CustomParameters.TryAdd(OccurrenceParameterKey, occurrence.ToString());
    }

    public void RequestNextEvent(string? id) => service.RequestNextEvent(id);
    public void RequestNextEventDelay(float delayDays) => History.RequestNextEventDelay(delayDays);
    public void ConcludeEvent(bool? doNotRepeat = null) => service.ConcludeEvent(this, doNotRepeat);
}
