namespace TimberPipes.Models;

public readonly record struct PipeHeadliftSource(float RatedLift, int OutletZ);

public static class PipeHeadlift
{
    public static bool CanLiftTo(int z, int outletZ, float ratedLift)
        => z - outletZ <= ratedLift;

    public static int PickInjectIndex(
        ReadOnlySpan<int> z,
        ReadOnlySpan<float> freeSpace,
        int outletZ,
        float ratedLift,
        float packet)
    {
        var best = -1;
        for (var i = 0; i < z.Length; i++)
        {
            if (freeSpace[i] < packet)
            {
                continue;
            }

            if (!CanLiftTo(z[i], outletZ, ratedLift))
            {
                continue;
            }

            if (best < 0
                || z[i] < z[best]
                || (z[i] == z[best] && freeSpace[i] > freeSpace[best]))
            {
                best = i;
            }
        }

        return best;
    }

    public static float ExtraAt(int z, List<PipeHeadliftSource> sources)
    {
        var extra = 0f;
        foreach (var source in sources)
        {
            extra = Math.Max(extra, PipeFlowSolver.RemainingLift(z, source.OutletZ, source.RatedLift));
        }

        return extra;
    }

    public static float ExtraAt(BuildingPipe pipe, PipeRegistry registry)
    {
        if (pipe.Graph is null || pipe.IsContaminated || pipe.Graph.Contaminated)
        {
            return 0f;
        }

        List<PipeHeadliftSource> sources = [];
        CollectSources(pipe.Graph, registry, sources);
        return ExtraAt(pipe.Coordinates.z, sources);
    }

    public static void CollectSources(PipeGraph graph, PipeRegistry registry, List<PipeHeadliftSource> sources)
    {
        sources.Clear();
        if (graph.Contaminated)
        {
            return;
        }

        foreach (var pipe in graph.Pipes.Values)
        {
            if (pipe.Ports is not { } ports)
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!registry.TryGetConnectedBuilding(port, out var other))
                {
                    continue;
                }

                var pump = other.GetComponentOrNull<PipePump>();
                if (pump is null || pump.IsPaused)
                {
                    continue;
                }

                if (pump.ColumnHeight > 0)
                {
                    sources.Add(new(pump.ColumnHeight, pump.OutletZ));
                }

                if (pump.RatedMaxHeadLift > 0)
                {
                    var outletZ = port.ConnectedPort?.Coordinates.z ?? other.Coordinates.z;
                    sources.Add(new(pump.RatedMaxHeadLift, outletZ));
                }
            }
        }
    }
}
