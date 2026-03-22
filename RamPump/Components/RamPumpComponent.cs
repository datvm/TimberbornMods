namespace RamPump.Components;

public class RamPumpComponent(
    RamPumpService service
) : TickableComponent, IAwakableComponent, IFinishedPausable, IFinishedStateListener, IEntityDescriber
{
    const float MinCurrent = 0.02f;
    const float ContaminationThreshold = 0.01f;

    public RamPumpSpec Spec { get; private set; } = null!;
    public Vector3Int InputCoord { get; private set; }
    Vector3Int outputCoord;

    float bufferClean;
    float bufferContaminated;

    public void Awake()
    {
        Spec = GetComponent<RamPumpSpec>();
        DisableComponent();
    }

    public void OnEnterFinishedState()
    {
        var bo = GetComponent<BlockObject>();

        InputCoord = bo.TransformCoordinates(Spec.InputChamber);
        outputCoord = bo.TransformCoordinates(Spec.OutputCoordinates);

        service.AddWaterLimit(this, bo);

        var blockable = GetComponent<BlockableObject>();
        blockable.ObjectBlocked += (_, _) => DisableComponent();
        blockable.ObjectUnblocked += (_, _) => EnableComponent();

        if (blockable.IsUnblocked)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
        }
    }

    public void OnExitFinishedState()
    {
        service.RemoveWaterLimit(this);
        DisableComponent();
    }

    public override void Tick()
    {
        // current is already m3 per tick
        var (total, contamination, current) = service.GetWaterAt(InputCoord);
        if (current <= MinCurrent || total <= 0) { return; }

        var taking = Mathf.Min(current * Spec.PumpPortion, total);

        var takingCont = contamination <= ContaminationThreshold ? 0 : taking * contamination;
        var takingClean = taking - takingCont;
        service.RemoveWater(InputCoord, takingClean, takingCont);

        bufferClean += takingClean;
        bufferContaminated += takingCont;

        if (bufferClean + bufferContaminated < Spec.Buffer) { return; }

        service.AddWater(outputCoord, bufferClean, bufferContaminated);
        bufferClean = 0;
        bufferContaminated = 0;
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [
        EntityDescription.CreateTextSection(service.GetStrengthDescription(Spec.PumpPortion), 100)
    ];
}
