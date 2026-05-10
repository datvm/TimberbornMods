namespace BeaverChronicles.Events;

public abstract class ChronicleEventBase : IChronicleEvent
{
    public virtual string Id { get; }
    public virtual string NameLoc => "LV.BCEv." + Id;

    public abstract IReadOnlyCollection<EventTriggerSource> TriggerSources { get; }
    public abstract void OnTriggered(IEventTriggerParameters parameters);

    public bool Active => chronicleEventService is not null && triggerParameters is not null;

    protected ChronicleEventService? chronicleEventService;
    protected IEventTriggerParameters? triggerParameters;
    protected EventHistoryRecord? historyRecord;

    public ChronicleEventBase()
    {
        Id = GetType().Name;
    }

    public abstract int GetTriggerWeight(IEventTriggerParameters parameters);

    public void Trigger(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        this.chronicleEventService = chronicleEventService;
        triggerParameters = parameters;
        historyRecord = chronicleEventService.ActiveRecord;

        OnTriggered(parameters);
    }

    protected virtual void Conclude()
    {
        if (!Active)
        {
            throw new InvalidOperationException("Cannot conclude an event that is not active.");
        }

        OnConcluded();
        chronicleEventService!.ConcludeEvent();

        Clear();
    }

    protected virtual void OnConcluded() { }

    void Clear()
    {
        triggerParameters = null;
        chronicleEventService = null;
        historyRecord = null;
    }

}
