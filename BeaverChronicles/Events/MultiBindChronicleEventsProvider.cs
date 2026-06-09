namespace BeaverChronicles.Events;

[MultiBind(typeof(IChronicleEventsProvider))]
public class MultiBindChronicleEventsProvider(IEnumerable<IChronicleEvent> events) : IChronicleEventsProvider
{
    public IEnumerable<IChronicleEvent> GetEvents() => events;
}
