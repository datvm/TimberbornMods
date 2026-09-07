namespace TimberPipes.Components;

[AddTemplateModule2(typeof(BuildingToPipeSpec))]
public class PipePump : BaseComponent, IAwakableComponent
{
#nullable disable
    BuildingToPipeSpec spec;
    BuildingPipe pipe;
    Inventories inventories;
#nullable enable

    MechanicalBuilding? mech;
    Workshop? workshop;
    Workplace? workplace;
    PausableBuilding? pausable;

    public BuildingPipe Pipe => pipe;
    public float RatedMaxHeadLift => spec.MaxHeadLift;
    public int? InjectRate => spec.InjectRate;

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

    public void Awake()
    {
        spec = GetComponent<BuildingToPipeSpec>();
        pipe = GetComponent<BuildingPipe>();
        inventories = GetComponent<Inventories>();
        mech = this.GetComponentOrNull<MechanicalBuilding>();
        workshop = this.GetComponentOrNull<Workshop>();
        workplace = this.GetComponentOrNull<Workplace>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }

    public void TryInject(PipeRegistry registry, int maxPackets)
    {
        if (maxPackets < 1 || WorkFactor <= 0 || EffectiveMaxHeadLift <= 0)
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

            if (!port.CanOutflow || !registry.TryGetConnectedBuilding(port, out var neighbor))
            {
                continue;
            }

            if (!neighbor.IsTransportPipe)
            {
                continue;
            }

            if (neighbor.Graph is { Contaminated: true })
            {
                continue;
            }

            var rise = neighbor.Coordinates.z - pipe.Coordinates.z;
            if (rise > EffectiveMaxHeadLift)
            {
                continue;
            }

            remaining -= InjectInto(neighbor, remaining);
        }
    }

    int InjectInto(BuildingPipe target, int maxPackets)
    {
        var injected = 0;
        var prefer = target.FluidGoodId ?? target.Graph?.FluidGoodId;

        while (injected < maxPackets && target.FreeSpace >= PipeFluids.PacketVolume)
        {
            if (!TryTakeGood(prefer, out var goodId))
            {
                break;
            }

            target.AddFluid(goodId, PipeFluids.PacketVolume);
            injected++;
        }

        return injected;
    }

    bool TryTakeGood(string? prefer, [NotNullWhen(true)] out string? goodId)
    {
        goodId = null;
        if (!inventories)
        {
            return false;
        }

        if (prefer is not null && TryTakeExact(prefer))
        {
            goodId = prefer;
            return true;
        }

        foreach (var inv in inventories.EnabledInventories)
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

                inv.TakeExisting(new(stock.GoodId, 1));
                goodId = stock.GoodId;
                return true;
            }
        }

        return false;
    }

    bool TryTakeExact(string goodId)
    {
        foreach (var inv in inventories.EnabledInventories)
        {
            if (!inv.IsOutput || !inv.HasUnreservedStock(goodId))
            {
                continue;
            }

            inv.TakeExisting(new(goodId, 1));
            return true;
        }

        return false;
    }
}
