namespace TimberPipes.Models;

public sealed class PipeGraph(FrozenDictionary<Vector3Int, BuildingPipe> pipes)
{
    public FrozenDictionary<Vector3Int, BuildingPipe> Pipes { get; } = pipes;

    public event EventHandler<BuildingPipe>? OnPortChanged; // Should not affect the graph

    public bool Contaminated { get; internal set; }
    public Dictionary<BuildingPipe, float> TransmittedLift { get; } = [];
    public PipeContaminationCause Cause { get; private set; }
    public string? FluidGoodId { get; private set; }

    internal void RaisePortChanged(BuildingPipe pipe) => OnPortChanged?.Invoke(this, pipe);

    public void AdoptFluid(string? id)
    {
        if (Contaminated || id is null || id.Length == 0 || id == PipeFluids.ContaminatedId)
        {
            return;
        }

        if (FluidGoodId is null)
        {
            FluidGoodId = id;
            return;
        }

        if (FluidGoodId != id)
        {
            Contaminate(new(FluidGoodId, id));
        }
    }

    public void RefreshContamination()
    {
        string? seen = null;
        foreach (var pipe in Pipes.Values)
        {
            if (pipe.IsContaminated)
            {
                Contaminate(Cause.HasPair ? Cause : pipe.ContaminationCause);
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
                Contaminate(new(seen, pipe.FluidGoodId));
                return;
            }
        }

        Contaminated = false;
        Cause = default;
        if (seen is not null)
        {
            AdoptFluid(seen);
        }
    }

    public void Contaminate(PipeContaminationCause cause)
    {
        if (!Contaminated)
        {
            Cause = cause;
            if (cause.HasPair)
            {
                Warn($"[TimberPipes] Contaminated ({Pipes.Count} pipes): mixed {cause.GoodA} with {cause.GoodB}");
            }
            else
            {
                Warn($"[TimberPipes] Contaminated ({Pipes.Count} pipes)");
            }
        }
        else if (!Cause.HasPair && cause.HasPair)
        {
            Cause = cause;
        }

        Contaminated = true;
        foreach (var pipe in Pipes.Values)
        {
            pipe.MarkContaminated(Cause);
        }
    }

    public void Flush()
    {
        foreach (var pipe in Pipes.Values)
        {
            pipe.ClearFluid();
        }

        Contaminated = false;
        Cause = default;
        FluidGoodId = null;
        TransmittedLift.Clear();
    }

    static void Warn(string message)
    {
        try
        {
            Debug.LogWarning(message);
        }
        catch
        {
            Console.Error.WriteLine(message);
        }
    }
}
