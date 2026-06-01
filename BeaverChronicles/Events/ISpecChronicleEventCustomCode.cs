namespace BeaverChronicles.Events;

public interface ISpecChronicleEventCustomCode
{
    string Id { get; }
    int GetWeight(SpecChronicleEvent ev);
    bool OnTrigger(SpecChronicleEvent ev);
    bool OnTriggerNode(SpecChronicleEvent ev, string? id);
    bool OnTriggerNode(SpecChronicleEvent ev, ChronicleEventNodeSpec node);
}
