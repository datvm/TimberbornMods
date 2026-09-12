namespace TimberPipes.Services;

[BindSingleton]
public class PipeFluidSimulator(
    PipeRegistry pipeRegistry,
    ISpecService specs,
    ITickService tick
) : ITickableSingleton, ILoadableSingleton
{
    PipeSimulationSpec spec = null!;

    public void Load()
    {
        spec = specs.GetSingleSpec<PipeSimulationSpec>();
    }

    public void Tick()
    {
        foreach (var valve in pipeRegistry.Valves)
        {
            valve.Pipe.PortState?.RefreshPortStatus();
        }

        foreach (var graph in pipeRegistry.Graphs)
        {
            graph.RefreshContamination();
        }

        foreach (var tank in pipeRegistry.Tanks)
        {
            tank.Quantize();
        }

        var packetRate = Math.Max(1, spec.DefaultSlurpRate);
        foreach (var valve in pipeRegistry.Valves)
        {
            valve.TryTransfer(packetRate);
        }

        foreach (var graph in pipeRegistry.Graphs)
        {
            graph.RefreshContamination();
            if (graph.Contaminated)
            {
                graph.TransmittedLift.Clear();
                continue;
            }

            Simulate(graph);
            graph.RefreshContamination();
        }
    }

    void Simulate(PipeGraph graph)
    {
        var flow = graph.Flow;
        if (flow.Dirty)
        {
            flow.Rebuild(graph, pipeRegistry);
        }

        var pipeCount = flow.PipeCount;
        if (pipeCount < 1)
        {
            graph.TransmittedLift.Clear();
            return;
        }

        var pipeGood = graph.FluidGoodId ?? NetworkGood(flow);
        foreach (var tank in flow.Tanks)
        {
            if (tank.ConflictsWith(pipeGood))
            {
                graph.Contaminate(new(tank.FluidGoodId, pipeGood));
                graph.TransmittedLift.Clear();
                return;
            }
        }

        var n = flow.FlowCount;
        var volumes = flow.Volumes;
        var capacities = flow.Capacities;
        var sourceLift = flow.SourceLift;
        var qMax = flow.QMax;
        var extra = flow.Extra;
        Array.Clear(sourceLift, 0, n);
        Array.Clear(qMax, 0, n);
        Array.Clear(extra, 0, n);

        var defaultGoods = Math.Max(1, spec.DefaultInjectRate);
        for (var i = 0; i < pipeCount; i++)
        {
            volumes[i] = flow.Pipes[i].FluidHeight;
            if (flow.Headlifts[i] is { } headlift)
            {
                sourceLift[i] = headlift.EffectiveMaxHeadLift;
                qMax[i] = PipeFlowSolver.FlowCap(headlift.InjectRate ?? defaultGoods, headlift.WorkFactor);
            }
        }

        for (var t = 0; t < flow.Tanks.Length; t++)
        {
            var tank = flow.Tanks[t];
            var start = flow.TankStarts[t];
            var slices = tank.SliceCount;
            var totalCap = Math.Max(tank.VolumeM3, tank.CapacityFor(graph.FluidGoodId ?? tank.FluidGoodId));
            var sliceCap = PipeFlowSolver.SliceCapacity(totalCap, tank.HeightTiles);
            PipeFlowSolver.PackSlices(tank.VolumeM3, volumes.AsSpan(start, slices), sliceCap);
            for (var s = 0; s < slices; s++)
            {
                capacities[start + s] = sliceCap;
            }
        }

        flow.RefreshEdgeAllows();
        var substeps = Math.Max(1, spec.Substeps);
        var kDt = spec.EqualizeK * tick.TickIntervalInSeconds / substeps;
        PipeFlowSolver.Run(
            volumes.AsSpan(0, n),
            capacities.AsSpan(0, n),
            flow.Z.AsSpan(0, n),
            flow.Edges.AsSpan(0, flow.EdgeCount),
            pipeCount,
            n,
            sourceLift.AsSpan(0, n),
            qMax.AsSpan(0, n),
            substeps,
            kDt,
            extra.AsSpan(0, n),
            flow.FillHeads,
            flow.Scratch);

        graph.TransmittedLift.Clear();
        for (var i = 0; i < pipeCount; i++)
        {
            if (extra[i] > 0)
            {
                graph.TransmittedLift[flow.Pipes[i]] = extra[i];
            }
        }

        var donor = graph.FluidGoodId;
        if (donor is null)
        {
            foreach (var tank in flow.Tanks)
            {
                if (tank.FluidGoodId is { } tankGood)
                {
                    donor = tankGood;
                    break;
                }
            }
        }

        graph.AdoptFluid(donor);
        for (var i = 0; i < pipeCount; i++)
        {
            flow.Pipes[i].SetVolume(volumes[i]);
            flow.Pipes[i].SyncNetworkGood();
        }

        for (var t = 0; t < flow.Tanks.Length; t++)
        {
            var tank = flow.Tanks[t];
            var start = flow.TankStarts[t];
            tank.ApplyVolume(
                PipeFlowSolver.UnpackSlices(volumes.AsSpan(start, tank.SliceCount)),
                donor ?? tank.FluidGoodId);
        }
    }

    static string? NetworkGood(PipeGraphFlowCache flow)
    {
        foreach (var pipe in flow.Pipes)
        {
            if (pipe.NetworkGoodId is { } id)
            {
                return id;
            }
        }

        return null;
    }
}
