namespace BeaverChronicles.Services.TimeLimitEvents;

public interface ITimeLimitEvent
{
    IEnumerable<string> ForEvents { get; }
    void SubscribeEvent(string name, SpecChronicleEventController controller);
    void UnsubscribeEvent(string name, SpecChronicleEventController controller);
}
