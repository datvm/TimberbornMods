namespace TimberPipes.Models;

public record PipeGraph(
    FrozenDictionary<Vector3Int, BuildingPipe> Pipes
)
{

    public event EventHandler<BuildingPipe>? OnPortChanged; // Should not affect the graph

    public bool Contaminated { get; internal set; }
    public float HeadLift { get; internal set; }

    internal void RaisePortChanged(BuildingPipe pipe) => OnPortChanged?.Invoke(this, pipe);

}
