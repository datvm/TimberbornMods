namespace BeaverChronicles.Events;

public abstract class DefaultSpecChronicleEventCustomCode : ISpecChronicleEventCustomCode
{
    public abstract string Id { get; }
    public virtual int GetWeight(SpecChronicleEvent ev) => 0;
    public bool OnTrigger(SpecChronicleEvent ev) => false;
    public bool OnTriggerNode(SpecChronicleEvent ev, string? id) => false;
    public bool OnTriggerNode(SpecChronicleEvent ev, ChronicleEventNodeSpec node) => false;
}
