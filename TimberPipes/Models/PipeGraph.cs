namespace TimberPipes.Models;

public record PipeGraph(
    FrozenDictionary<Vector3Int, BuildingPipe> Pipes
)
{

    public event EventHandler<BuildingPipe>? OnPortChanged; // Should not affect the graph

    public bool Contaminated { get; internal set; }

    public string? FluidGoodId
    {
        get
        {
            foreach (var pipe in Pipes.Values)
            {
                if (pipe.FluidGoodId is not null && !pipe.IsContaminated)
                {
                    return pipe.FluidGoodId;
                }
            }

            return null;
        }
    }

    internal void RaisePortChanged(BuildingPipe pipe) => OnPortChanged?.Invoke(this, pipe);

    public void RefreshContamination()
    {
        if (Contaminated)
        {
            Contaminate();
            return;
        }

        string? seen = null;
        foreach (var pipe in Pipes.Values)
        {
            if (pipe.IsContaminated)
            {
                Contaminate();
                return;
            }

            if (pipe.FluidGoodId is null)
            {
                continue;
            }

            if (seen is null)
            {
                seen = pipe.FluidGoodId;
            }
            else if (seen != pipe.FluidGoodId)
            {
                Contaminate();
                return;
            }
        }
    }

    public void Contaminate()
    {
        Contaminated = true;
        foreach (var pipe in Pipes.Values)
        {
            pipe.MarkContaminated();
        }
    }

    public void Flush()
    {
        foreach (var pipe in Pipes.Values)
        {
            pipe.ClearFluid();
        }

        Contaminated = false;
    }

}
