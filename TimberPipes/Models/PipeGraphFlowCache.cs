namespace TimberPipes.Models;

readonly record struct PipeFlowLink(int A, int B, PipePort? Port, bool Internal);

public sealed class PipeGraphFlowCache
{
    public bool Dirty { get; set; } = true;
    public BuildingPipe[] Pipes { get; private set; } = [];
    public PipeTank[] Tanks { get; private set; } = [];
    public int[] TankStarts { get; private set; } = [];
    public HeadliftPipe?[] Headlifts { get; private set; } = [];
    public int PipeCount { get; private set; }
    public int FlowCount { get; private set; }
    public int EdgeCount { get; private set; }

    public float[] Volumes { get; private set; } = [];
    public float[] Capacities { get; private set; } = [];
    public int[] Z { get; private set; } = [];
    public float[] SourceLift { get; private set; } = [];
    public float[] QMax { get; private set; } = [];
    public float[] Extra { get; private set; } = [];
    public PipeFlowEdge[] Edges { get; private set; } = [];
    public PipeFlowScratch Scratch { get; } = new();
    public PipeHeadFill FillHeads { get; }

    PipeFlowLink[] links = [];

    public PipeGraphFlowCache()
    {
        FillHeads = FillHeadsCore;
    }

    public void Rebuild(PipeGraph graph, PipeRegistry registry)
    {
        List<BuildingPipe> pipes = [.. graph.Pipes.Values];
        PipeCount = pipes.Count;
        Pipes = [.. pipes];
        Headlifts = new HeadliftPipe?[PipeCount];
        Dictionary<BuildingPipe, int> pipeIndex = [];
        for (var i = 0; i < PipeCount; i++)
        {
            pipeIndex[Pipes[i]] = i;
            Headlifts[i] = Pipes[i].Headlift;
        }

        List<PipeTank> tanks = [];
        Dictionary<PipeTank, int> tankStart = [];
        foreach (var pipe in Pipes)
        {
            if (pipe.Ports is not { } ports)
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !registry.TryGetConnectedBuilding(port, out var other) || other.IsTransportPipe)
                {
                    continue;
                }

                if (other.Tank is not { } tank)
                {
                    continue;
                }

                if (tankStart.TryAdd(tank, 0))
                {
                    tanks.Add(tank);
                }
            }
        }

        Tanks = [.. tanks];
        TankStarts = new int[Tanks.Length];
        var sliceCount = 0;
        for (var t = 0; t < Tanks.Length; t++)
        {
            TankStarts[t] = PipeCount + sliceCount;
            tankStart[Tanks[t]] = TankStarts[t];
            sliceCount += Tanks[t].SliceCount;
        }

        FlowCount = PipeCount + sliceCount;
        EnsureBuffers(FlowCount);

        for (var i = 0; i < PipeCount; i++)
        {
            Capacities[i] = BuildingPipe.MaxWaterHeight;
            Z[i] = Pipes[i].Coordinates.z;
        }

        for (var t = 0; t < Tanks.Length; t++)
        {
            var tank = Tanks[t];
            var start = TankStarts[t];
            for (var s = 0; s < tank.SliceCount; s++)
            {
                Z[start + s] = tank.ZBase + s;
            }
        }

        List<PipeFlowLink> built = [];
        HashSet<long> seen = [];
        foreach (var pipe in Pipes)
        {
            if (pipe.Ports is not { } ports || !pipeIndex.TryGetValue(pipe, out var i))
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !registry.TryGetConnectedBuilding(port, out var other))
                {
                    continue;
                }

                var j = IndexOf(other, port, pipeIndex, tankStart);
                if (j < 0 || i == j || !seen.Add(EdgeKey(i, j)))
                {
                    continue;
                }

                built.Add(new(i, j, port, Internal: false));
            }
        }

        for (var t = 0; t < Tanks.Length; t++)
        {
            var start = TankStarts[t];
            var slices = Tanks[t].SliceCount;
            for (var s = 0; s < slices - 1; s++)
            {
                var i = start + s;
                var j = i + 1;
                if (!seen.Add(EdgeKey(i, j)))
                {
                    continue;
                }

                built.Add(new(i, j, null, Internal: true));
            }
        }

        links = [.. built];
        EdgeCount = links.Length;
        if (Edges.Length < EdgeCount)
        {
            Edges = new PipeFlowEdge[EdgeCount];
        }

        Dirty = false;
    }

    public void RefreshEdgeAllows()
    {
        for (var e = 0; e < EdgeCount; e++)
        {
            var link = links[e];
            Edges[e] = link.Internal
                ? new(link.A, link.B, true, true)
                : new(link.A, link.B, link.Port!.CanOutflow, link.Port.CanInflow);
        }
    }

    void FillHeadsCore(Span<float> heads, ReadOnlySpan<float> current)
    {
        for (var i = 0; i < FlowCount; i++)
        {
            if (i < PipeCount)
            {
                heads[i] = PipeFlowSolver.PipeHead(Z[i], current[i]);
            }
            else
            {
                heads[i] = PipeFlowSolver.Surface(Z[i], current[i], Capacities[i]);
            }
        }
    }

    void EnsureBuffers(int n)
    {
        if (Volumes.Length >= n)
        {
            return;
        }

        Volumes = new float[n];
        Capacities = new float[n];
        Z = new int[n];
        SourceLift = new float[n];
        QMax = new float[n];
        Extra = new float[n];
    }

    int IndexOf(
        BuildingPipe other,
        PipePort port,
        Dictionary<BuildingPipe, int> pipeIndex,
        Dictionary<PipeTank, int> tankStart)
    {
        if (pipeIndex.TryGetValue(other, out var i))
        {
            return i;
        }

        if (other.Tank is { } tank && tankStart.TryGetValue(tank, out var start))
        {
            return start + tank.SliceAt(port.GetOppositePortDefinition().Coordinates.z);
        }

        return -1;
    }

    static long EdgeKey(int i, int j)
        => i < j ? ((long)i << 32) | (uint)j : ((long)j << 32) | (uint)i;
}
