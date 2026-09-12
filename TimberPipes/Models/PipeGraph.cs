namespace TimberPipes.Models;

public sealed class PipeGraph(FrozenDictionary<Vector3Int, BuildingPipe> pipes)
{
    public FrozenDictionary<Vector3Int, BuildingPipe> Pipes { get; } = pipes;
    public BuildingPipe? StatusPipe { get; } = FindStatusPipe(pipes);
    public PipeGraphFlowCache Flow { get; } = new();

    public event EventHandler<BuildingPipe>? OnPortChanged; // Should not affect the graph membership

    public bool Contaminated { get; internal set; }
    public Dictionary<BuildingPipe, float> TransmittedLift { get; } = [];
    public PipeContaminationCause Cause { get; private set; }
    public string? FluidGoodId { get; private set; }

    internal void RaisePortChanged(BuildingPipe pipe)
    {
        Flow.Dirty = true;
        OnPortChanged?.Invoke(this, pipe);
    }

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

        RefreshStatusIcons();
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

        RefreshStatusIcons();
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
        RefreshStatusIcons();
    }

    public bool IsStatusPipe(BuildingPipe pipe) => StatusPipe == pipe;

    public static Vector3Int? StatusCoordinates(IEnumerable<Vector3Int> coordinates)
    {
        Vector3Int? first = null;
        foreach (var c in coordinates)
        {
            if (first is null || CompareCoordinates(c, first.Value) < 0)
            {
                first = c;
            }
        }

        return first;
    }

    public static int CompareCoordinates(Vector3Int a, Vector3Int b)
    {
        var x = a.x.CompareTo(b.x);
        if (x != 0)
        {
            return x;
        }

        var y = a.y.CompareTo(b.y);
        if (y != 0)
        {
            return y;
        }

        return a.z.CompareTo(b.z);
    }

    internal void RefreshStatusIcons()
    {
        foreach (var pipe in Pipes.Values)
        {
            pipe.RefreshContaminationStatus();
        }
    }

    static BuildingPipe? FindStatusPipe(FrozenDictionary<Vector3Int, BuildingPipe> pipes)
    {
        if (StatusCoordinates(pipes.Keys) is not { } coords)
        {
            return null;
        }

        return pipes[coords];
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
