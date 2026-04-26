namespace BeaverChronicles.Events.Implementations;

public class StartBonusResourcesEvent : ChronicleEventBase
{
    public override IReadOnlyCollection<EventTriggerSource> TriggerSources => [EventTriggerSource.NewDay];

    public override int GetTriggerWeight(IEventTriggerParameters parameters) => int.MaxValue;

    public override void OnTriggered(IEventTriggerParameters parameters)
    {

    }
}
