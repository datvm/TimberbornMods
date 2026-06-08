namespace BeaverChronicles.Events;

public abstract class ChronicleEventBase : IChronicleEvent
{
    public virtual string Id { get; }
    public virtual string NameLoc => "LV.BCEv." + Id;

    public abstract IReadOnlyCollection<EventTriggerSource> TriggerSources { get; }
    protected abstract void OnTriggered(IEventTriggerParameters parameters, EventHistoryRecord record);

    public bool Active => context is not null && triggerParameters is not null;
    public virtual float? DelayAfterConclusion => 0f;

    protected ChronicleEventContext? context;
    protected IEventTriggerParameters? triggerParameters;
    protected EventHistoryRecord? historyRecord;

    public ChronicleEventBase()
    {
        Id = GetType().Name;
    }

    public abstract int GetTriggerWeight(ChronicleEventContext context);

    public void Trigger(ChronicleEventContext context)
    {
        this.context = context;
        triggerParameters = context.Parameters;
        historyRecord = context.ActiveRecord;

        OnTriggered(context.Parameters, historyRecord!);
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
            context!.RequestNextEventDelay(delay.Value);
        }

        OnConcluded();
        context!.ConcludeEvent();

        Clear();
    }

    protected virtual void OnConcluded() { }

    void Clear()
    {
        triggerParameters = null;
        context = null;
        historyRecord = null;
    }

    protected string GetParameter(string key) => historyRecord!.CustomParameters[key];
    protected void SetParameter(string key, string value) => historyRecord!.CustomParameters[key] = value;

}
