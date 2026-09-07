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
        foreach (var building in buildings)
        {
            building.GetComponentOrNull<BuildingPipePortState>()?.RefreshPortStatus();
        }

        foreach (var graph in pipeRegistry.Graphs)
        {
            graph.RefreshContamination();
        }

        foreach (var building in buildings)
        {
            building.GetComponentOrNull<PipeTank>()?.Quantize();
            building.GetComponentOrNull<PipePump>()?.Quantize();
        }

        var injectRate = Math.Max(1, spec.DefaultInjectRate);
        var slurpRate = Math.Max(1, spec.DefaultSlurpRate);

        foreach (var building in buildings)
        {
            if (building.GetComponentOrNull<PipePump>() is { } pump)
            {
                pump.TryInject(pipeRegistry, pump.InjectRate ?? injectRate);
            }
        }

        foreach (var building in buildings)
        {
            if (building.GetComponentOrNull<PipeDump>() is { } dump)
            {
                dump.TrySlurp(pipeRegistry, dump.SlurpRate ?? slurpRate);
            }
        }

        foreach (var graph in pipeRegistry.Graphs)
        {
            graph.RefreshContamination();
            if (graph.Contaminated)
            {
                continue;
            }

            Equalize(graph);
            graph.RefreshContamination();
        }
    }

    void Equalize(PipeGraph graph)
    {
        List<BuildingPipe> nodes = [.. graph.Pipes.Values];
        var pipeCount = nodes.Count;
        if (pipeCount < 1)
        {
            return;
        }

        Dictionary<BuildingPipe, int> index = [];
        for (var i = 0; i < nodes.Count; i++)
        {
            index[nodes[i]] = i;
        }

        List<PipeTank?> tanks = [];
        List<PipePump?> pumps = [];
        for (var i = 0; i < nodes.Count; i++)
        {
            tanks.Add(null);
            pumps.Add(null);
        }

        foreach (var pipe in graph.Pipes.Values)
        {
            if (pipe.Ports is not { } ports)
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !pipeRegistry.TryGetConnectedBuilding(port, out var other))
                {
                    continue;
                }

                var tank = other.GetComponentOrNull<PipeTank>();
                var pump = other.GetComponentOrNull<PipePump>();
                if (tank is null && pump is null)
                {
                    continue;
                }

                if (tank is not null && tank.ConflictsWith(pipe.FluidGoodId ?? graph.FluidGoodId))
                {
                    graph.Contaminate();
                    return;
                }

                if (pump is not null && pump.ConflictsWith(pipe.FluidGoodId ?? graph.FluidGoodId))
                {
                    graph.Contaminate();
                    return;
                }

                if (!index.ContainsKey(other))
                {
                    index[other] = nodes.Count;
                    nodes.Add(other);
                    tanks.Add(tank);
                    pumps.Add(pump);
                }
            }
        }

        var volumes = new float[nodes.Count];
        var capacities = new float[nodes.Count];
        var pipeZ = new int[pipeCount];
        for (var i = 0; i < nodes.Count; i++)
        {
            if (i < pipeCount)
            {
                volumes[i] = nodes[i].FluidHeight;
                capacities[i] = BuildingPipe.MaxWaterHeight;
                pipeZ[i] = nodes[i].Coordinates.z;
            }
            else if (tanks[i] is { } tank)
            {
                volumes[i] = tank.VolumeM3;
                capacities[i] = Math.Max(tank.VolumeM3, tank.CapacityFor(graph.FluidGoodId ?? tank.FluidGoodId));
            }
            else
            {
                var pump = pumps[i]!;
                volumes[i] = pump.VolumeM3;
                capacities[i] = Math.Max(pump.VolumeM3, pump.CapacityFor(graph.FluidGoodId ?? pump.FluidGoodId));
            }
        }

        List<PipeFlowEdge> edges = [];
        HashSet<long> seen = [];
        foreach (var pipe in graph.Pipes.Values)
        {
            if (pipe.Ports is not { } ports || !index.TryGetValue(pipe, out var i))
            {
                continue;
            }

            foreach (var port in ports.Values)
            {
                if (!port.IsConnected || !pipeRegistry.TryGetConnectedBuilding(port, out var other))
                {
                    continue;
                }

                if (!index.TryGetValue(other, out var j) || i == j)
                {
                    continue;
                }

                if (!port.CanOutflow && !port.CanInflow)
                {
                    continue;
                }

                var key = i < j ? ((long)i << 32) | (uint)j : ((long)j << 32) | (uint)i;
                if (!seen.Add(key))
                {
                    continue;
                }

                edges.Add(i < j
                    ? new(i, j, port.CanOutflow, port.CanInflow)
                    : new(j, i, port.CanInflow, port.CanOutflow));
            }
        }

        if (edges.Count < 1)
        {
            return;
        }

        var edgeArray = edges.ToArray();
        var heads = new float[nodes.Count];
        var substeps = Math.Max(1, spec.Substeps);
        var kDt = spec.EqualizeK * tick.TickIntervalInSeconds / substeps;
        for (var step = 0; step < substeps; step++)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (i < pipeCount)
                {
                    heads[i] = PipeFlowSolver.PipeHead(pipeZ[i], volumes[i]);
                }
                else if (tanks[i] is { } tank)
                {
                    heads[i] = PipeFlowSolver.TankHead(tank.ZBase, volumes[i], capacities[i], tank.HeightTiles);
                }
                else
                {
                    heads[i] = PipeFlowSolver.PumpHead(pumps[i]!.OutletZ, volumes[i]);
                }
            }

            PipeFlowSolver.Equalize(volumes, heads, capacities, edgeArray, kDt);
        }

        var donor = graph.FluidGoodId;
        if (donor is null)
        {
            foreach (var tank in tanks)
            {
                if (tank?.FluidGoodId is { } tankGood)
                {
                    donor = tankGood;
                    break;
                }
            }

            if (donor is null)
            {
                foreach (var pump in pumps)
                {
                    if (pump?.FluidGoodId is { } pumpGood)
                    {
                        donor = pumpGood;
                        break;
                    }
                }
            }
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            if (i < pipeCount)
            {
                var pipe = nodes[i];
                if (volumes[i] > pipe.FluidHeight && pipe.FluidGoodId is null && donor is not null)
                {
                    pipe.AssignFluidId(donor);
                }

                pipe.SetVolume(volumes[i]);
            }
            else if (tanks[i] is { } tank)
            {
                tank.ApplyVolume(volumes[i], donor ?? tank.FluidGoodId);
            }
            else
            {
                var pump = pumps[i]!;
                pump.ApplyVolume(volumes[i], donor ?? pump.FluidGoodId);
            }
        }
    }
}
