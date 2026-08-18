namespace TimberPipes.Models;

public record PipePort(PipePortDefinition Definition, PipePortSpec PortSpec)
{
    public Vector3Int Coordinates => Definition.Coordinates;
    public Direction3D Direction => Definition.Direction;

    public PipePortState? OverrideState { get; set; }

    public PipePortState State { get; internal set; } = PipePortState.Closed;
    public PipePortConnection? Connection { get; internal set; }
    public PipePort? ConnectedPort => Connection?.GetOther(this);
    public bool IsConnected => Connection is not null;

    public bool AllowsInflow => (State & PipePortState.OpenIn) != 0;
    public bool AllowsOutflow => (State & PipePortState.OpenOut) != 0;

    public PipePortDefinition GetOppositePortDefinition() => new(Coordinates + Direction.ToOffset(), Direction.Across());

    public bool CanOutflow => AllowsOutflow && ConnectedPort is { } port && port.AllowsInflow;
    public bool CanInflow => AllowsInflow && ConnectedPort is { } port && port.AllowsOutflow;
}

public readonly record struct PipePortDefinition(Vector3Int Coordinates, Direction3D Direction);