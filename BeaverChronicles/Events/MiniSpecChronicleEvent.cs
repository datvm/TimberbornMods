namespace BeaverChronicles.Events;

public class MiniSpecChronicleEvent(ChronicleEventSpec spec, ISpecChronicleEventCustomCode? customCode, SpecChronicleEventControllerFactory helperFac)
    : SpecChronicleEvent(spec, customCode, helperFac), IMiniChronicleEvent
{
    public bool IsMini => true;
}