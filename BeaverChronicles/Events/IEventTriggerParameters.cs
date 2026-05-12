namespace BeaverChronicles.Events;

public interface IEventTriggerParameters
{
    static readonly IEventTriggerParameters GameLoad = new EventTriggerParameter<bool>(EventTriggerSource.GameLoad, 0, true);

    EventTriggerSource Source { get; }

}

public readonly record struct EventTriggerParameter<T>(EventTriggerSource Source, int SourceCount, T Data) : IEventTriggerParameters;