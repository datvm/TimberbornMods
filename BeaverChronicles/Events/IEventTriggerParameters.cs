namespace BeaverChronicles.Events;

public interface IEventTriggerParameters
{
    static readonly IEventTriggerParameters GameLoad = new EventTriggerParameter<bool>(EventTriggerSource.GameLoad, 0, true);

    EventTriggerSource Source { get; }
    object? DataObject { get; }

}

public interface IEventTriggerParameters<T> : IEventTriggerParameters
{
    int SourceCount { get; }
    T Data { get; }
}

public interface IEventSpecificTriggerParameters : IEventTriggerParameters
{
    IChronicleEvent Event { get; }
}

public interface ITriggerParameterWith<out T>
{
    T Parameter { get; }
}

public readonly record struct EventTriggerParameter<T>(EventTriggerSource Source, int SourceCount, T Data) : IEventTriggerParameters<T>
{
    public object? DataObject => Data;
}

public readonly record struct EventSpecificTriggerParameter<T>(EventTriggerSource Source, int SourceCount, T Data, IChronicleEvent Event) : IEventSpecificTriggerParameters, IEventTriggerParameters<T>
{
    public object? DataObject => Data;
}
