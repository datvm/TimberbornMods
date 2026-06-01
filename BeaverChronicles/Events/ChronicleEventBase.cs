namespace BeaverChronicles.Events;

public abstract class ChronicleEventBase : IChronicleEvent
{
    public virtual string Id { get; }
    public virtual string NameLoc => "LV.BCEv." + Id;

    public abstract IReadOnlyCollection<EventTriggerSource> TriggerSources { get; }
    protected abstract void OnTriggered(IEventTriggerParameters parameters, EventHistoryRecord record);

    public bool Active => chronicleEventService is not null && triggerParameters is not null;
    public virtual float? DelayAfterConclusion => 0f;

    protected ChronicleEventService? chronicleEventService;
    protected IEventTriggerParameters? triggerParameters;
    protected EventHistoryRecord? historyRecord;

    public ChronicleEventBase()
    {
        Id = GetType().Name;
    }

    public abstract int GetTriggerWeight(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService);

    public void Trigger(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        this.chronicleEventService = chronicleEventService;
        triggerParameters = parameters;
        historyRecord = chronicleEventService.ActiveRecord;

        OnTriggered(parameters, historyRecord!);
    }

    protected virtual void Conclude()
    {
        if (!Active)
        {
            throw new InvalidOperationException("Cannot conclude an event that is not active.");
        }

        var delay = DelayAfterConclusion;
        if (delay.HasValue)
        {
            chronicleEventService!.RequestNextEventDelay(delay.Value);
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

    protected string GetParameter(string key) => historyRecord!.CustomParameters[key];
    protected void SetParameter(string key, string value) => historyRecord!.CustomParameters[key] = value;

}
