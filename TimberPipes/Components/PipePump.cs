namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingToPipeSpec))]
public class PipePump : BaseComponent, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PipePump));
    static readonly PropertyKey<float> PendingKey = new("PendingVolume");

#nullable disable
    BuildingPipe pipe;
    BlockObject bo;
    Inventories inventories;
#nullable enable

    BuildingToPipeSpec? spec;

    MechanicalBuilding? mech;
    Workshop? workshop;
    Workplace? workplace;
    PausableBuilding? pausable;
    float pending;

    public BuildingPipe Pipe => pipe;
    public float RatedMaxHeadLift => spec?.MaxHeadLift ?? 0f;
    public int? InjectRate => spec?.InjectRate;
    public bool IsPaused => pausable is { Paused: true };

    public int OutletZ => LowestOutletZ(null);

    public int OutletZFor(PipeGraph graph) => LowestOutletZ(graph);

    public float ColumnHeight => Math.Clamp(VolumeM3 / PipeFluids.PipeCapacity, 0f, 1f);
    public float Head => PipeFlowSolver.PumpHead(OutletZ, VolumeM3);
    public float DisplayMaxHeadLift => IsPaused ? 0f : Math.Max(RatedMaxHeadLift, ColumnHeight);

    public float WorkFactor
    {
        get
        {
            if (pausable is { Paused: true })
            {
                return 0f;
            }

            if (mech)
            {
                return mech!.ActiveAndPowered ? mech.Efficiency : 0f;
            }

            if (workshop)
            {
                return workshop!.CurrentlyWorking ? 1f : 0f;
            }

            if (workplace)
            {
                foreach (var worker in workplace!.AssignedWorkers)
                {
                    if (worker.JobRunning)
                    {
                        return 1f;
                    }
                }

                return 0f;
            }

            return 1f;
        }
    }

    public float EffectiveMaxHeadLift => RatedMaxHeadLift * WorkFactor;

    public string? FluidGoodId
    {
        get
        {
            foreach (var inv in EnabledInventories())
            {
                foreach (var stock in inv.UnreservedStock())
                {
                    if (stock.Amount > 0)
                    {
                        return stock.GoodId;
                    }
                }
            }

            return null;
        }
    }

    public float VolumeM3 => PipeFlowSolver.GoodsToVolume(StoredGoods) + pending;

    public void Awake()
    {
        spec = TryGetComponent<BuildingToPipeSpec>(out var toPipe) ? toPipe : null;
        pipe = GetComponent<BuildingPipe>();
        bo = GetComponent<BlockObject>();
        inventories = GetComponent<Inventories>();
        mech = this.GetComponentOrNull<MechanicalBuilding>();
        workshop = this.GetComponentOrNull<Workshop>();
        workplace = this.GetComponentOrNull<Workplace>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }

    public bool TakesGood(string goodId)
    {
        foreach (var inv in EnabledInventories())
        {
            if (inv.Takes(goodId))
            {
                return true;
            }
        }

        return false;
    }

    public bool ConflictsWith(string? pipeGoodId)
        => PipeFlowSolver.WellConflictsWithPipe(FluidGoodId, pipeGoodId);

    public float CapacityFor(string? goodId)
        => PipeFlowSolver.GoodsToVolume(Math.Max(StoredGoods + FreeGoods(goodId ?? FluidGoodId), 0));

    public void ApplyVolume(float volumeM3, string? incomingGoodId)
    {
        pending = volumeM3 - PipeFlowSolver.GoodsToVolume(StoredGoods);
        var delta = PipeFlowSolver.QuantizePending(ref pending);
        if (delta > 0)
        {
            if (incomingGoodId is not null && !TakesGood(incomingGoodId))
            {
                pending = 0;
                return;
            }

            TryGive(incomingGoodId ?? FluidGoodId, delta);
        }
        else if (delta < 0)
        {
            TryTake(-delta);
        }
    }

    public void Quantize()
    {
        var delta = PipeFlowSolver.QuantizePending(ref pending);
        if (delta > 0)
        {
            TryGive(FluidGoodId, delta);
        }
        else if (delta < 0)
        {
            TryTake(-delta);
        }
    }

    int LowestOutletZ(PipeGraph? graph)
    {
        if (pipe.Ports is not { } ports)
        {
            return bo.CoordinatesAtBaseZ.z;
        }

        var z = int.MaxValue;
        foreach (var port in ports.Values)
        {
            if (!port.IsConnected || !port.AllowsOutflow)
            {
                continue;
            }

            if (graph is not null && !graph.Pipes.ContainsKey(port.GetOppositePortDefinition().Coordinates))
            {
                continue;
            }

            z = Math.Min(z, port.Coordinates.z);
        }

        return z == int.MaxValue ? bo.CoordinatesAtBaseZ.z : z;
    }

    int StoredGoods
    {
        get
        {
            var total = 0;
            foreach (var inv in EnabledInventories())
            {
                total += inv.TotalAmountInStock;
            }

            return total;
        }
    }

    int FreeGoods(string? goodId)
    {
        var free = 0;
        foreach (var inv in EnabledInventories())
        {
            if (goodId is null)
            {
                free += Math.Max(0, inv.Capacity - inv.TotalAmountInStock);
                continue;
            }

            if (inv.Takes(goodId))
            {
                free += inv.UnreservedCapacity(goodId);
            }
        }

        return free;
    }

    void TryGive(string? goodId, int amount)
    {
        if (goodId is null || amount < 1)
        {
            return;
        }

        var left = amount;
        foreach (var inv in EnabledInventories())
        {
            if (!inv.Takes(goodId))
            {
                continue;
            }

            var space = inv.UnreservedCapacity(goodId);
            if (space < 1)
            {
                continue;
            }

            var n = Math.Min(left, space);
            inv.GiveExisting(new(goodId, n));
            left -= n;
            if (left < 1)
            {
                return;
            }
        }

        pending += PipeFlowSolver.GoodsToVolume(left);
    }

    void TryTake(int amount)
    {
        var left = amount;
        foreach (var inv in EnabledInventories())
        {
            if (!inv.IsOutput)
            {
                continue;
            }

            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount < 1)
                {
                    continue;
                }

                var n = Math.Min(left, stock.Amount);
                inv.TakeExisting(new(stock.GoodId, n));
                left -= n;
                if (left < 1)
                {
                    return;
                }
            }
        }

        pending -= PipeFlowSolver.GoodsToVolume(left);
    }

    IEnumerable<Inventory> EnabledInventories()
    {
        if (!inventories)
        {
            yield break;
        }

        foreach (var inv in inventories.EnabledInventories)
        {
            yield return inv;
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        entitySaver.GetComponent(SaveKey).Set(PendingKey, pending);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        pending = s.Get(PendingKey);
    }
}
