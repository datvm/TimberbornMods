namespace TimberPipes.Components;

[AddTemplateModule2(typeof(FluidBufferBuildingSpec))]
public class PipeTank : BaseComponent, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(PipeTank));
    static readonly PropertyKey<float> PendingKey = new("PendingVolume");

#nullable disable
    FluidBufferBuildingSpec spec;
    BuildingPipe pipe;
    BlockObject bo;
    Inventories inventories;
#nullable enable

    float pending;

    public BuildingPipe Pipe => pipe;
    public int ZBase => bo.Coordinates.z;
    public int HeightTiles => spec.Height ?? Math.Max(1, bo.Blocks.Size.z);

    public void Awake()
    {
        spec = GetComponent<FluidBufferBuildingSpec>();
        pipe = GetComponent<BuildingPipe>();
        bo = GetComponent<BlockObject>();
        inventories = GetComponent<Inventories>();
    }

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
    public float CapacityM3 => PipeFlowSolver.GoodsToVolume(Math.Max(StoredGoods + FreeGoods(FluidGoodId), 0));
    public float Head => PipeFlowSolver.TankHead(ZBase, VolumeM3, CapacityM3, HeightTiles);

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
        pending = volumeM3 - PipeFlowSolver.GoodsToVolume(StoredGoods);
        var delta = PipeFlowSolver.QuantizePending(ref pending);
        if (delta > 0)
        {
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
