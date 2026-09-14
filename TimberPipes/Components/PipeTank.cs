namespace TimberPipes.Components;

[AddTemplateModule2(typeof(TankPipe))]
public class PipeTank : BaseComponent, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PipeTank));
    static readonly PropertyKey<float> PendingKey = new("PendingVolume");

#nullable disable
    BuildingPipe pipe;
    BlockObject bo;
    Inventories inventories;
#nullable enable

    float pending;

    public BuildingPipe Pipe => pipe;
    public int ZBase => bo.Coordinates.z;
    public int HeightTiles => Math.Max(1, bo.Blocks.Size.z);
    public int SliceCount => PipeFlowSolver.SliceCount(HeightTiles);

    public int SliceAt(int worldZ) => PipeFlowSolver.SliceIndex(worldZ, ZBase, HeightTiles);

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        bo = GetComponent<BlockObject>();
        inventories = GetComponent<Inventories>();
        if (TryGetComponent<StockpileSpec>(out var stockpile)
            && stockpile.WhitelistedGoodType != PipeFluids.LiquidGoodType)
        {
            DisableComponent();
        }
    }

    public string? FluidGoodId
    {
        get
        {
            foreach (var inv in EnabledInventories())
            {
                foreach (var stock in inv.Stock)
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
    public float CapacityM3 => PipeFlowSolver.GoodsToVolume(Math.Max(StoredGoods + FreeGoods(FluidGoodId), 0));
    public float FlowVolumeM3 => PipeTankIo.FlowVolume(DrainableGoods, pending);
    public float Head => PipeFlowSolver.TankHead(ZBase, VolumeM3, CapacityM3, HeightTiles);
    public float FillHeight => Math.Max(0f, Head - ZBase);

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
        => PipeFlowSolver.TankConflictsWithPipe(FluidGoodId, pipeGoodId is not null && TakesGood(pipeGoodId), pipeGoodId);

    public float CapacityFor(string? goodId)
        => PipeTankIo.FlowCapacity(DrainableGoods, FreeGoods(goodId ?? FluidGoodId));

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

    public void ApplyVolume(float volumeM3, string? incomingGoodId)
    {
        pending = PipeTankIo.PendingFromSolver(volumeM3, DrainableGoods);
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

    int DrainableGoods
    {
        get
        {
            var total = 0;
            foreach (var inv in EnabledInventories())
            {
                if (!inv.IsOutput)
                {
                    continue;
                }

                foreach (var stock in inv.UnreservedTakeableStock())
                {
                    total += stock.Amount;
                }
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
                free += PipeTankIo.FillableGoods(
                    inv.Capacity,
                    inv.TotalAmountInStock,
                    ReservedCapacityAmount(inv));
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
        if (amount < 1)
        {
            return;
        }

        if (goodId is null)
        {
            pending += PipeFlowSolver.GoodsToVolume(amount);
            return;
        }

        var left = amount;
        foreach (var inv in EnabledInventories())
        {
            if (!inv.Takes(goodId))
            {
                continue;
            }

            var n = PipeTankIo.GiveCount(left, inv.UnreservedCapacity(goodId));
            if (n < 1)
            {
                continue;
            }

            inv.GiveExisting(new(goodId, n));
            left -= n;
            if (left < 1)
            {
                return;
            }
        }
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
                var n = PipeTankIo.TakeCount(left, stock.Amount);
                if (n < 1)
                {
                    continue;
                }

                inv.TakeExisting(new(stock.GoodId, n));
                left -= n;
                if (left < 1)
                {
                    return;
                }
            }
        }
    }

    static int ReservedCapacityAmount(Inventory inv)
    {
        var n = 0;
        foreach (var good in inv.ReservedCapacity())
        {
            n += good.Amount;
        }

        return n;
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
        if (!Enabled)
        {
            return;
        }

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
