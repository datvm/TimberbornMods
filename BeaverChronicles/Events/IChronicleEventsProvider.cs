namespace BeaverChronicles.Events;

public interface IChronicleEventsProvider
{
    IEnumerable<IChronicleEvent> GetEvents();
}
