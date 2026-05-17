namespace MoreHttpApi.Shared;

public record HttpAutomationMap(
    IReadOnlyList<HttpAutomator> Automators,
    HttpBuildingsResult Buildings
);

public record HttpAutomator(
    HttpEntityModel Entity,
    HttpAutomatorKind Kind,

    HttpColor Color,

    HttpAutomatorState State,
    bool IsCyclicOrBlocked,

    IReadOnlyList<HttpAutomationInput> Inputs
);

public record HttpAutomationInput(
    Guid? FromAutomatorId,
    HttpAutomationConnectionState State
);

public enum HttpAutomatorKind
{
    Terminal,
    Transmitter,
    SamplingTransmitter,
    CombinationalTransmitter,
    SequentialTransmitter
}

public enum HttpAutomatorState
{
    Off,
    On,
    Error,
}

public enum HttpAutomationConnectionState
{
    Disconnected,
    Off,
    On
}
