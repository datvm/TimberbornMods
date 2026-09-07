namespace TimberPipes.Components;

[AddTemplateModule2(typeof(PipeToBuildingSpec))]
public class PipeDump : BaseComponent, IAwakableComponent
{
#nullable disable
    PipeToBuildingSpec spec;
    BuildingPipe pipe;
    Inventories inventories;
#nullable enable

    PausableBuilding? pausable;

    public BuildingPipe Pipe => pipe;
    public int? SlurpRate => spec.SlurpRate;

    public void Awake()
    {
        spec = GetComponent<PipeToBuildingSpec>();
        pipe = GetComponent<BuildingPipe>();
        inventories = GetComponent<Inventories>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }

    public void TrySlurp(PipeRegistry registry, int maxPackets)
    {
        if (maxPackets < 1 || pausable is { Paused: true })
        {
            return;
        }

        if (pipe.Ports is not { } ports)
        {
            return;
        }

        var remaining = maxPackets;
        foreach (var port in ports.Values)
        {
            if (remaining < 1)
            {
                return;
            }

            if (!port.CanInflow || !registry.TryGetConnectedBuilding(port, out var neighbor))
            {
                continue;
            }

            if (!neighbor.IsTransportPipe || neighbor.IsContaminated || neighbor.Graph is { Contaminated: true })
            {
                continue;
            }

            remaining -= SlurpFrom(neighbor, remaining);
        }
    }

    int SlurpFrom(BuildingPipe source, int maxPackets)
    {
        var slurped = 0;
        while (slurped < maxPackets
            && source.FluidHeight >= PipeFluids.PacketVolume
            && source.FluidGoodId is { } goodId
            && !source.IsContaminated)
        {
            if (!TryGiveGood(goodId))
            {
                break;
            }

            source.RemoveFluid(PipeFluids.PacketVolume);
            slurped++;
        }

        return slurped;
    }

    bool TryGiveGood(string goodId)
    {
        if (!inventories)
        {
            return false;
        }

        var packet = new GoodAmount(goodId, 1);
        foreach (var inv in inventories.EnabledInventories)
        {
            if (!inv.IsInput || !inv.HasUnreservedCapacity(packet))
            {
                continue;
            }

            inv.GiveExisting(packet);
            return true;
        }

        return false;
    }
}
