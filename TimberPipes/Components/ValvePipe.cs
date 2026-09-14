namespace TimberPipes.Components;

[AddTemplateModule2(typeof(ValvePipeSpec))]
public class ValvePipe(ValvePipeService service) : BaseComponent, IFinishedPausable, IAwakableComponent, IPersistentEntity, IDuplicable<ValvePipe>
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

    public BuildingPipe Pipe => pipe;

    public bool InletEnabled { get; set; }
    public bool OutletEnabled { get; set; }
    public string? OutletGoodId { get; set; }

    float extractBuffer;
    string? extractBufferGoodId;
    IBuildingPipeConnection? inletTarget;
    IBuildingPipeConnection? outletTarget;
    bool inletCached;
    bool outletCached;

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }

    public IBuildingPipeConnection? FindInletTarget()
        => CachedTarget(ref inletTarget, ref inletCached, PipePortState.OpenOut, give: true);

    public IBuildingPipeConnection? FindOutletTarget()
        => CachedTarget(ref outletTarget, ref outletCached, PipePortState.OpenIn, give: false);

    public void InvalidateIoTargets()
    {
        inletTarget = null;
        outletTarget = null;
        inletCached = false;
        outletCached = false;
    }

    public bool FacesCell(Vector3Int cell)
    {
        if (pipe.Ports is not { } ports)
        {
            return false;
        }

        foreach (var port in ports.Values)
        {
            if ((port.PortSpec.State & (PipePortState.OpenIn | PipePortState.OpenOut)) == 0)
            {
                continue;
            }

            if (port.GetOppositePortDefinition().Coordinates == cell)
            {
                return true;
            }
        }

        return false;
    }

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
        => service.ExtractDropdownGoods(FindOutletTarget(), OutletGoodId);

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
            && target.TryTransfer(goodId, 1))
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
            && target.TryTransfer(goodId, 1))
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

    IBuildingPipeConnection? CachedTarget(
        ref IBuildingPipeConnection? cached,
        ref bool resolved,
        PipePortState required,
        bool give)
    {
        if (resolved && (cached is null || cached.IsValid))
        {
            return cached;
        }

        cached = service.FindConnection(pipe, required, give);
        resolved = true;
        return cached;
    }

    public void DuplicateFrom(ValvePipe source)
    {
        CopySettings(source.InletEnabled, source.OutletEnabled, source.OutletGoodId);
    }

    public void CopySettings(bool inletEnabled, bool outletEnabled, string? outletGoodId)
    {
        InletEnabled = inletEnabled;
        OutletEnabled = outletEnabled;
        var goodId = outletGoodId is { Length: > 0 } ? outletGoodId : null;
        if (OutletGoodId != goodId)
        {
            extractBuffer = 0f;
            extractBufferGoodId = null;
        }

        OutletGoodId = goodId;
    }

    bool PipeContaminated => pipe.IsContaminated || pipe.Graph is { Contaminated: true };
}
