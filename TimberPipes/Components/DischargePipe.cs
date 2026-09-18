namespace TimberPipes.Components;

[AddTemplateModule2(typeof(DischargePipeSpec))]
public class DischargePipe(DischargePipeService service, IDayNightCycle dayNight)
    : BaseComponent, IFinishedPausable, IAwakableComponent, IInitializableEntity, IOutflowCounter
{
    static readonly Vector3Int LocalEject = new(0, 1, 0);

#nullable disable
    BuildingPipe pipe;
    BlockObject bo;
    DischargePipeSpec spec;
#nullable enable

    BlockableObject? blockable;
    WaterOutput? waterOutput;
    Vector3Int ejectCell;
    readonly OutflowHourWindow window = new();

    public BuildingPipe Pipe => pipe;

    public bool IsEjecting { get; private set; }

    public float HourVolume
    {
        get
        {
            window.Prune(dayNight.PartialDayNumber);
            return window.HourVolume;
        }
    }

    public float TickVolume { get; private set; }

    public void AddOutflow(float volumeM3) => window.Add(dayNight.PartialDayNumber, volumeM3);

    public void SetTickOutflow(float volumeM3)
    {
        TickVolume = Math.Max(0f, volumeM3);
        AddOutflow(TickVolume);
    }

    public void Prune(float nowPartialDay) => window.Prune(nowPartialDay);

    public void Awake()
    {
        pipe = GetComponent<BuildingPipe>();
        bo = GetComponent<BlockObject>();
        spec = GetComponent<DischargePipeSpec>();
        blockable = this.GetComponentOrNull<BlockableObject>();
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
        if (blockable is { IsUnblocked: false }
            || pipe.IsContaminated
            || pipe.Graph is { Contaminated: true })
        {
            IsEjecting = false;
            SetTickOutflow(0f);
            return;
        }

        var goodId = pipe.NetworkGoodId;
        var space = service.AvailableSpace(ejectCell, spec.DistanceToGroundOffset);
        var amount = service.EjectAmount(IsEjecting, pipe.FluidHeight, goodId, space, spec.EjectBuffer);
        if (amount <= 0)
        {
            IsEjecting = false;
            SetTickOutflow(0f);
            return;
        }

        pipe.RemoveFluid(amount);
        SetTickOutflow(amount);
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
