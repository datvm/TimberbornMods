namespace TimberPipes.Components;

[AddTemplateModule2(typeof(DischargePipeSpec))]
public class DischargePipe(DischargePipeService service)
    : BaseComponent, IFinishedPausable, IAwakableComponent, IInitializableEntity
{
    static readonly Vector3Int LocalEject = new(0, 1, 0);

#nullable disable
    BuildingPipe pipe;
    BlockObject bo;
    DischargePipeSpec spec;
#nullable enable

    PausableBuilding? pausable;
    WaterOutput? waterOutput;
    Vector3Int ejectCell;

    public BuildingPipe Pipe => pipe;

    public bool IsEjecting { get; private set; }

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        bo = GetComponent<BlockObject>();
        spec = GetComponent<DischargePipeSpec>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
        waterOutput = this.GetComponentOrNull<WaterOutput>();
    }

    public void InitializeEntity()
    {
        ejectCell = bo.TransformCoordinates(LocalEject);
    }

    public void CheckContamination()
    {
        var goodId = pipe.NetworkGoodId ?? pipe.FluidGoodId;
        if (!DischargePipeIo.ShouldContaminate(goodId))
        {
            return;
        }

        pipe.Graph?.Contaminate(DischargePipeIo.WrongFluidCause(goodId!));
    }

    public void TryEject()
    {
        if (pausable is { Paused: true }
            || pipe.IsContaminated
            || pipe.Graph is { Contaminated: true })
        {
            IsEjecting = false;
            return;
        }

        var goodId = pipe.NetworkGoodId;
        var space = service.AvailableSpace(ejectCell, spec.DistanceToGroundOffset);
        var amount = service.EjectAmount(IsEjecting, pipe.FluidHeight, goodId, space, spec.EjectBuffer);
        if (amount <= 0)
        {
            IsEjecting = false;
            return;
        }

        pipe.RemoveFluid(amount);
        if (DischargePipeIo.IsContaminatedWater(goodId))
        {
            service.AddWorldWater(waterOutput, ejectCell, clean: 0f, contaminated: amount);
        }
        else
        {
            service.AddWorldWater(waterOutput, ejectCell, clean: amount, contaminated: 0f);
        }

        IsEjecting = true;
    }

}
