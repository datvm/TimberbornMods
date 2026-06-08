namespace BeaverChronicles.Events;

[NonBindingEvent]
public class SpecChronicleEvent(
    ChronicleEventSpec spec,
    ISpecChronicleEventCustomCode? customCode,
    SpecChronicleEventHelperFactory helperFac
) : IChronicleEvent
{
    public ChronicleEventSpec Spec => spec;
    public string Id => spec.Id;
    public string NameLoc => spec.TitleLoc;
    public IReadOnlyCollection<EventTriggerSource> TriggerSources => spec.Conditions.Sources;
    public bool Active { get; private set; }
    public bool CanRepeat => spec.Repeat;

    ChronicleEventContext? context;
    IEventTriggerParameters? parameters;
    SpecChronicleEventHelper? helper;

    public ChronicleEventContext Context => context ?? throw Throw();
    public IEventTriggerParameters Parameters => parameters ?? throw Throw();
    public SpecChronicleEventHelper Helper => helper ?? throw Throw();
    public ISpecChronicleEventCustomCode? CustomCode => customCode;
    static InvalidOperationException Throw() => new("Event is not active");

    public int GetTriggerWeight(ChronicleEventContext context)
    {
        SetContext(context);

        return helper!.CheckFlags() switch
        {
            SpecChronicleEventHelper.FlagCheckResult.Block => -1,
            SpecChronicleEventHelper.FlagCheckResult.CannotTrigger => 0,
            _ => spec.Conditions.CustomWeightCode ? customCode!.GetWeight(this) : spec.Conditions.Weight,
        };
    }

    public void Trigger(ChronicleEventContext context)
    {
        SetContext(context);
        Active = true;

        if (context.Parameters.Source == EventTriggerSource.GameLoad)
        {
            helper!.RestoreGameState();
            return;
        }

        if (customCode?.OnTrigger(this) == true) { return; }

        var startNodeId = spec.Nodes.StartNodeId;
        if (string.IsNullOrEmpty(startNodeId))
        {
            throw new InvalidOperationException($"Event {Id} has no start node and no custom code");
        }
        TriggerNode(startNodeId);
    }

    public void TriggerNode(string? id)
    {
        if (customCode?.OnTriggerNode(this, id) == true) { return; }

        if (string.IsNullOrEmpty(id))
        {
            Conclude();
            return;
        }

        if (!spec.Nodes.TryGetNode(id, out var node))
        {
            throw new InvalidOperationException($"Event {Id} has no node with id {id} and no custom code to handle it");
        }

        TriggerNode(node);
    }

    void TriggerNode(ChronicleEventNodeSpec node)
    {
        if (customCode?.OnTriggerNode(this, node) == true) { return; }

        helper!.TriggerNode(node);
    }

    void SetContext(ChronicleEventContext context)
    {
        this.context = context;
        parameters = context.Parameters;
        helper = helperFac.Create(this, context);
    }

    void Conclude()
    {
        context!.ConcludeEvent();
        Active = false;

        context = null;
        parameters = null;
        helper = null;
    }

}
