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
        foreach (var graph in pipeRegistry.Graphs)
        {
            graph.RefreshContamination();
        }

        List<BuildingPipe> buildings = [.. pipeRegistry.All];

        foreach (var building in buildings)
        {
            building.GetComponentOrNull<PipeTank>()?.Quantize();
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
        Dictionary<BuildingPipe, int> index = [];
        for (var i = 0; i < nodes.Count; i++)
        {
            index[nodes[i]] = i;
        }

        List<PipeTank?> tanks = [];
        for (var i = 0; i < nodes.Count; i++)
        {
            tanks.Add(null);
        }

        List<PipeFlowEdge> edges = [];

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
                if (other.IsTransportPipe)
                {
                    if (!IsCanonical(port, port.ConnectedPort!))
                    {
                        continue;
                    }

                    if (!index.TryGetValue(other, out var otherIndex))
                    {
                        continue;
                    }

                    edges.Add(new(
                        index[pipe],
                        otherIndex,
                        port.CanOutflow,
                        port.CanInflow
                    ));
                    continue;
                }

                if (tank is null)
                {
                    continue;
                }

                if (tank.FluidGoodId is { } tankGood
                    && pipe.FluidGoodId is { } pipeGood
                    && tankGood != pipeGood)
                {
                    graph.Contaminate();
                    return;
                }

                if (!index.TryGetValue(other, out var tankIndex))
                {
                    tankIndex = nodes.Count;
                    index[other] = tankIndex;
                    nodes.Add(other);
                    tanks.Add(tank);
                }

                edges.Add(new(
                    index[pipe],
                    tankIndex,
                    port.CanOutflow,
                    port.CanInflow
                ));
            }
        }

        if (edges.Count == 0)
        {
            return;
        }

        var volumes = new float[nodes.Count];
        var heads = new float[nodes.Count];
        var capacities = new float[nodes.Count];
        for (var i = 0; i < nodes.Count; i++)
        {
            var tank = tanks[i];
            if (tank is null)
            {
                volumes[i] = nodes[i].FluidHeight;
                capacities[i] = BuildingPipe.MaxWaterHeight;
            }
            else
            {
                volumes[i] = tank.VolumeM3;
                capacities[i] = Math.Max(tank.VolumeM3, tank.CapacityM3);
            }
        }

        var substeps = Math.Max(1, spec.Substeps);
        var kDt = spec.EqualizeK * tick.TickIntervalInSeconds / substeps;
        var edgeArray = edges.ToArray();

        for (var step = 0; step < substeps; step++)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var tank = tanks[i];
                heads[i] = tank is null
                    ? PipeFlowSolver.PipeHead(nodes[i].Coordinates.z, volumes[i])
                    : PipeFlowSolver.TankHead(tank.ZBase, volumes[i], capacities[i], tank.HeightTiles);
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
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            var tank = tanks[i];
            if (tank is null)
            {
                var pipe = nodes[i];
                if (volumes[i] > pipe.FluidHeight && pipe.FluidGoodId is null && donor is not null)
                {
                    pipe.AssignFluidId(donor);
                }

                pipe.SetVolume(volumes[i]);
            }
            else
            {
                tank.ApplyVolume(volumes[i], donor ?? tank.FluidGoodId);
            }
        }
    }

    static bool IsCanonical(PipePort a, PipePort b)
    {
        var c = a.Coordinates.x.CompareTo(b.Coordinates.x);
        if (c != 0)
        {
            return c < 0;
        }

        c = a.Coordinates.y.CompareTo(b.Coordinates.y);
        if (c != 0)
        {
            return c < 0;
        }

        c = a.Coordinates.z.CompareTo(b.Coordinates.z);
        if (c != 0)
        {
            return c < 0;
        }

        return (int)a.Direction <= (int)b.Direction;
    }

}
