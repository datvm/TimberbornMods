namespace TimberPipes.Components;

[AddTemplateModule2(typeof(ValvePipeSpec))]
public class ValvePipe(IBlockService blockService, IGoodService goods) : BaseComponent, IFinishedPausable, IAwakableComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(ValvePipe));
    static readonly PropertyKey<bool> InletEnabledKey = new("InletEnabled");
    static readonly PropertyKey<bool> OutletEnabledKey = new("OutletEnabled");
    static readonly PropertyKey<string> OutletGoodIdKey = new("OutletGoodId");
    static readonly PropertyKey<float> ExtractBufferKey = new("ExtractBuffer");
    static readonly PropertyKey<string> ExtractBufferGoodIdKey = new("ExtractBufferGoodId");

#nullable disable
    BuildingPipe pipe;
#nullable enable

    PausableBuilding? pausable;

    public bool InletEnabled { get; set; }
    public bool OutletEnabled { get; set; }
    public string? OutletGoodId { get; set; }

    float extractBuffer;
    string? extractBufferGoodId;

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }

    public ValveIoTarget? FindInletTarget() => FindTarget(PipePortState.OpenOut, fill: true);

    public ValveIoTarget? FindOutletTarget() => FindTarget(PipePortState.OpenIn, fill: false);

    public void TryTransfer(int maxPackets)
    {
        if (maxPackets < 1 || pausable is { Paused: true })
        {
            return;
        }

        TryInlet(maxPackets);
        TryOutlet(maxPackets);
    }

    public List<string> OutletGoodIds()
    {
        if (FindOutletTarget() is not { } target)
        {
            return [];
        }

        var liquids = LiquidIdSet();
        var known = ValvePipeIo.KnownExtractLiquids(
            OutputGoodIds(target.Inventories),
            TakeableGoodIds(target.Inventories),
            liquids);
        return ValvePipeIo.ExtractDropdownGoods(known, [.. liquids]);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(InletEnabledKey, InletEnabled);
        s.Set(OutletEnabledKey, OutletEnabled);
        s.Set(OutletGoodIdKey, OutletGoodId ?? "");
        s.Set(ExtractBufferKey, extractBuffer);
        s.Set(ExtractBufferGoodIdKey, extractBufferGoodId ?? "");
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(InletEnabledKey))
        {
            InletEnabled = s.Get(InletEnabledKey);
        }

        if (s.Has(OutletEnabledKey))
        {
            OutletEnabled = s.Get(OutletEnabledKey);
        }

        if (s.Has(OutletGoodIdKey))
        {
            var id = s.Get(OutletGoodIdKey);
            OutletGoodId = id is { Length: > 0 } ? id : null;
        }

        if (s.Has(ExtractBufferKey))
        {
            extractBuffer = s.Get(ExtractBufferKey);
        }

        if (s.Has(ExtractBufferGoodIdKey))
        {
            var id = s.Get(ExtractBufferGoodIdKey);
            extractBufferGoodId = id is { Length: > 0 } ? id : null;
        }
    }

    void TryInlet(int maxPackets)
    {
        var remaining = maxPackets;
        while (remaining > 0
            && ValvePipeIo.CanInlet(InletEnabled, false, PipeContaminated, pipe.FluidHeight, pipe.NetworkGoodId)
            && FindInletTarget() is { } target
            && pipe.NetworkGoodId is { } goodId
            && TryGiveGood(target.Inventories, goodId))
        {
            pipe.RemoveFluid(PipeFluids.PacketVolume);
            remaining--;
        }
    }

    void TryOutlet(int maxPackets)
    {
        PourExtractBuffer();
        var remaining = maxPackets;
        while (remaining > 0
            && ValvePipeIo.CanOutlet(OutletEnabled, false, PipeContaminated, pipe.FreeSpace, OutletGoodId)
            && extractBuffer <= PipeFluids.MoveEpsilon
            && FindOutletTarget() is { } target
            && OutletGoodId is { } goodId
            && TryTakeGood(target.Inventories, goodId))
        {
            extractBuffer += PipeFluids.PacketVolume;
            extractBufferGoodId = goodId;
            PourExtractBuffer();
            remaining--;
        }
    }

    void PourExtractBuffer()
    {
        if (!ValvePipeIo.CanPourExtractBuffer(false, extractBuffer, pipe.FreeSpace))
        {
            return;
        }

        var goodId = extractBufferGoodId ?? OutletGoodId;
        if (goodId is null || goodId.Length == 0)
        {
            return;
        }

        var amount = ValvePipeIo.ExtractPourAmount(extractBuffer, pipe.FreeSpace);
        if (amount <= 0f)
        {
            return;
        }

        pipe.AddFluid(goodId, amount);
        extractBuffer -= amount;
        if (extractBuffer <= PipeFluids.MoveEpsilon)
        {
            extractBuffer = 0f;
            extractBufferGoodId = null;
        }
    }

    ValveIoTarget? FindTarget(PipePortState required, bool fill)
    {
        if (pipe.Ports is not { } ports)
        {
            return null;
        }

        foreach (var port in ports.Values)
        {
            if ((port.PortSpec.State & required) == 0)
            {
                continue;
            }

            if (TryGetCandidate(port.GetOppositePortDefinition().Coordinates, fill, out var target))
            {
                return target;
            }
        }

        return null;
    }

    bool TryGetCandidate(Vector3Int cell, bool fill, out ValveIoTarget target)
    {
        target = default;
        foreach (var obj in blockService.GetObjectsAt(cell))
        {
            if (obj.Overridable)
            {
                continue;
            }

            var inventories = obj.GetComponent<Inventories>();
            if (!ValvePipeIo.IsBuildingCandidate(
                obj.IsFinished,
                inventories && ValvePipeIo.HasActiveInventory(inventories.EnabledInventories.Count),
                obj.HasComponent<TransportPipeSpec>(),
                obj.GetComponent<PipeTank>() is { Enabled: true }))
            {
                continue;
            }

            var liquids = LiquidIdSet();
            if (fill)
            {
                if (!ValvePipeIo.HasLiquidInput(InputGoodIds(inventories), liquids))
                {
                    continue;
                }
            }
            else if (ValvePipeIo.KnownExtractLiquids(
                OutputGoodIds(inventories),
                TakeableGoodIds(inventories),
                liquids).Count == 0)
            {
                continue;
            }

            target = new(obj, inventories);
            return true;
        }

        return false;
    }

    HashSet<string> LiquidIdSet()
    {
        HashSet<string> ids = [];
        foreach (var id in goods.GetGoodsForType(PipeFluids.LiquidGoodType))
        {
            if (goods.HasGood(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    static List<string> InputGoodIds(Inventories inventories)
    {
        List<string> ids = [];
        foreach (var inv in inventories.EnabledInventories)
        {
            foreach (var id in inv.InputGoods)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    static List<string> OutputGoodIds(Inventories inventories)
    {
        List<string> ids = [];
        foreach (var inv in inventories.EnabledInventories)
        {
            foreach (var id in inv.OutputGoods)
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    static List<string> TakeableGoodIds(Inventories inventories)
    {
        List<string> ids = [];
        foreach (var inv in inventories.EnabledInventories)
        {
            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.Amount > 0)
                {
                    ids.Add(stock.GoodId);
                }
            }
        }

        return ids;
    }

    bool PipeContaminated => pipe.IsContaminated || pipe.Graph is { Contaminated: true };

    static bool TryGiveGood(Inventories inventories, string goodId)
    {
        var packet = new GoodAmount(goodId, 1);
        foreach (var inv in inventories.EnabledInventories)
        {
            if (!ValvePipeIo.CanGiveToBuilding(inv.Takes(goodId), inv.HasUnreservedCapacity(packet)))
            {
                continue;
            }

            inv.GiveExisting(packet);
            return true;
        }

        return false;
    }

    static bool TryTakeGood(Inventories inventories, string goodId)
    {
        var packet = new GoodAmount(goodId, 1);
        foreach (var inv in inventories.EnabledInventories)
        {
            foreach (var stock in inv.UnreservedTakeableStock())
            {
                if (stock.GoodId != goodId || !ValvePipeIo.CanTakeFromBuilding(stock.Amount))
                {
                    continue;
                }

                inv.TakeExisting(packet);
                return true;
            }
        }

        return false;
    }
}
