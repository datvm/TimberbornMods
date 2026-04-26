namespace BeaverChronicles.Events;

public interface IEventTriggerParameters
{
    static readonly IEventTriggerParameters GameLoad = new EventTriggerParameter<bool>(EventTriggerSource.GameLoad, true);

    EventTriggerSource Source { get; }
}

public readonly record struct EventTriggerParameter<T>(EventTriggerSource Source, T Data) : IEventTriggerParameters;