namespace BeaverChronicles.Events;

public class SpecChronicleEvent(
    ChronicleEventSpec spec,
    ISpecChronicleEventCustomCode? customCode,
    SpecChronicleEventControllerFactory helperFac
) : IChronicleEvent
{
    public ChronicleEventSpec Spec => spec;
    public string Id => spec.Id;
    public string NameLoc => spec.TitleLoc;
    public IReadOnlyCollection<EventTriggerSource> TriggerSources => spec.Conditions.Sources;
    public bool Active => Controller.IsActive;
    public bool CanRepeat => spec.Repeat;

    public SpecChronicleEventController Controller { get; private set; } = null!;
    public ISpecChronicleEventCustomCode? CustomCode => customCode;

    internal void Initialize()
    {
        if (Spec.Conditions.NeedCustomCode && customCode is null)
        {
            throw new InvalidOperationException($"Event {Id} requires custom code, but none was provided.");
        }

        Controller = helperFac.Create(this);
    }

    public int GetTriggerWeight(ChronicleTriggerContext context) 
        => Controller.GetTriggerWeight(context);

    public void Trigger(ChronicleEventContext context)
    {
        Controller.SetContext(context);        
        if (context.Parameters.Source == EventTriggerSource.GameLoad)
        {
            Controller.RestoreGameState();
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

        Controller.TriggerNode(node);
    }

    void Conclude()
    {
        Controller.ConcludeEvent();
    }

}
