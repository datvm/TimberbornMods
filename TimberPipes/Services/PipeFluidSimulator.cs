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
        List<BuildingPipe> buildings = [.. pipeRegistry.All];
        List<PipeGraph> graphs = [.. pipeRegistry.Graphs];
        foreach (var building in buildings)
        {
            building.GetComponentOrNull<BuildingPipePortState>()?.RefreshPortStatus();
        }

        foreach (var graph in graphs)
        {
            graph.RefreshContamination();
        }

        foreach (var building in buildings)
        {
            building.GetComponentOrNull<PipeTank>()?.Quantize();
            building.GetComponentOrNull<PipePump>()?.Quantize();
        }

        var slurpRate = Math.Max(1, spec.DefaultSlurpRate);
        foreach (var building in buildings)
        {
            if (building.GetComponentOrNull<PipeDump>() is { } dump)
            {
                dump.TrySlurp(pipeRegistry, dump.SlurpRate ?? slurpRate);
            }

            building.GetComponentOrNull<ValvePipe>()?.TryTransfer(slurpRate);
        }

        foreach (var graph in graphs)
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
        List<BuildingPipe> pipes = [.. graph.Pipes.Values];
        var pipeCount = pipes.Count;
        if (pipeCount < 1)
        {
            graph.TransmittedLift.Clear();
            return;
        }

        Dictionary<BuildingPipe, int> pipeIndex = [];
        for (var i = 0; i < pipes.Count; i++)
        {
            pipeIndex[pipes[i]] = i;
        }

        List<PipeTank> tanks = [];
        Dictionary<PipeTank, int> tankStart = [];
        foreach (var pipe in pipes)
        {
            if (pipe.Ports is not { } ports)
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !pipeRegistry.TryGetConnectedBuilding(port, out var other) || other.IsTransportPipe)
                {
                    continue;
                }

                if (other.GetComponentOrNull<PipeTank>() is not { } tank)
                {
                    continue;
                }

                if (tank.ConflictsWith(graph.FluidGoodId ?? pipe.NetworkGoodId))
                {
                    graph.Contaminate(new(tank.FluidGoodId, graph.FluidGoodId ?? pipe.NetworkGoodId));
                    graph.TransmittedLift.Clear();
                    return;
                }

                if (tankStart.TryAdd(tank, 0))
                {
                    tanks.Add(tank);
                }
            }
        }

        var sliceCount = 0;
        foreach (var tank in tanks)
        {
            tankStart[tank] = pipeCount + sliceCount;
            sliceCount += tank.SliceCount;
        }

        var flowCount = pipeCount + sliceCount;
        var n = flowCount;
        var volumes = new float[n];
        var capacities = new float[n];
        var z = new int[n];
        var sourceLift = new float[n];
        var qMax = new float[n];
        var defaultGoods = Math.Max(1, spec.DefaultInjectRate);
        for (var i = 0; i < pipeCount; i++)
        {
            volumes[i] = pipes[i].FluidHeight;
            capacities[i] = BuildingPipe.MaxWaterHeight;
            z[i] = pipes[i].Coordinates.z;
            if (pipes[i].GetComponentOrNull<HeadliftPipe>() is { } headlift)
            {
                sourceLift[i] = headlift.EffectiveMaxHeadLift;
                qMax[i] = PipeFlowSolver.FlowCap(headlift.InjectRate ?? defaultGoods, headlift.WorkFactor);
            }
        }

        foreach (var tank in tanks)
        {
            var start = tankStart[tank];
            var slices = tank.SliceCount;
            var totalCap = Math.Max(tank.VolumeM3, tank.CapacityFor(graph.FluidGoodId ?? tank.FluidGoodId));
            var sliceCap = PipeFlowSolver.SliceCapacity(totalCap, tank.HeightTiles);
            var packed = new float[slices];
            PipeFlowSolver.PackSlices(tank.VolumeM3, packed, sliceCap);
            for (var s = 0; s < slices; s++)
            {
                volumes[start + s] = packed[s];
                capacities[start + s] = sliceCap;
                z[start + s] = tank.ZBase + s;
            }
        }

        List<PipeFlowEdge> edges = [];
        HashSet<long> seen = [];
        foreach (var pipe in pipes)
        {
            if (pipe.Ports is not { } ports || !pipeIndex.TryGetValue(pipe, out var i))
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !pipeRegistry.TryGetConnectedBuilding(port, out var other))
                {
                    continue;
                }

                var j = IndexOf(other, port);
                if (j < 0 || i == j || (!port.CanOutflow && !port.CanInflow))
                {
                    continue;
                }

                if (!seen.Add(EdgeKey(i, j)))
                {
                    continue;
                }

                edges.Add(i < j
                    ? new(i, j, port.CanOutflow, port.CanInflow)
                    : new(j, i, port.CanInflow, port.CanOutflow));
            }
        }

        foreach (var tank in tanks)
        {
            var start = tankStart[tank];
            var slices = tank.SliceCount;
            for (var s = 0; s < slices - 1; s++)
            {
                var i = start + s;
                var j = i + 1;
                if (!seen.Add(EdgeKey(i, j)))
                {
                    continue;
                }

                edges.Add(new(i, j, true, true));
            }
        }

        var extra = new float[n];
        var substeps = Math.Max(1, spec.Substeps);
        var kDt = spec.EqualizeK * tick.TickIntervalInSeconds / substeps;
        PipeFlowSolver.Run(
            volumes,
            capacities,
            z,
            edges.ToArray(),
            pipeCount,
            flowCount,
            sourceLift,
            qMax,
            substeps,
            kDt,
            extra,
            (heads, current) =>
            {
                for (var i = 0; i < n; i++)
                {
                    if (i < pipeCount)
                    {
                        heads[i] = PipeFlowSolver.PipeHead(z[i], current[i]);
                    }
                    else
                    {
                        heads[i] = PipeFlowSolver.Surface(z[i], current[i], capacities[i]);
                    }
                }
            });

        graph.TransmittedLift.Clear();
        for (var i = 0; i < pipeCount; i++)
        {
            if (extra[i] > 0)
            {
                graph.TransmittedLift[pipes[i]] = extra[i];
            }
        }

        var donor = graph.FluidGoodId;
        if (donor is null)
        {
            foreach (var tank in tanks)
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
            pipes[i].SetVolume(volumes[i]);
            pipes[i].SyncNetworkGood();
        }

        foreach (var tank in tanks)
        {
            var start = tankStart[tank];
            tank.ApplyVolume(
                PipeFlowSolver.UnpackSlices(volumes.AsSpan(start, tank.SliceCount)),
                donor ?? tank.FluidGoodId);
        }

        int IndexOf(BuildingPipe other, PipePort port)
        {
            if (pipeIndex.TryGetValue(other, out var i))
            {
                return i;
            }

            if (other.GetComponentOrNull<PipeTank>() is { } tank && tankStart.TryGetValue(tank, out var start))
            {
                return start + tank.SliceAt(port.GetOppositePortDefinition().Coordinates.z);
            }

            return -1;
        }
    }

    static long EdgeKey(int i, int j)
        => i < j ? ((long)i << 32) | (uint)j : ((long)j << 32) | (uint)i;
}
