namespace BeaverChronicles.Events;

public abstract class ChronicleEventBase : IChronicleEvent
{
    public virtual string Id => GetType().FullName;
    public abstract IReadOnlyCollection<EventTriggerSource> TriggerSources { get; }
    public abstract void OnTriggered(IEventTriggerParameters parameters);

    public bool Active => chronicleEventService is not null && triggerParameters is not null;

    protected ChronicleEventService? chronicleEventService;
    protected IEventTriggerParameters? triggerParameters;

    public abstract int GetTriggerWeight(IEventTriggerParameters parameters);

    public void Trigger(IEventTriggerParameters parameters, ChronicleEventService chronicleEventService)
    {
        this.chronicleEventService = chronicleEventService;
        triggerParameters = parameters;

        OnTriggered(parameters);
    }

    protected virtual void Conclude()
    {
        if (!Active)
        {
            throw new InvalidOperationException("Cannot conclude an event that is not active.");
        }

        OnConcluded();

        Clear();
        chronicleEventService!.ConcludeEvent();
    }

    protected virtual void OnConcluded() { }

    void Clear()
    {
        triggerParameters = null;
        chronicleEventService = null;
    }

}
