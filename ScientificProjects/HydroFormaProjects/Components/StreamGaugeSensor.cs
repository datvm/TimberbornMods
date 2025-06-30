namespace HydroFormaProjects.Components;

public class StreamGaugeSensor : TickableComponent, ISelectionListener
{

#nullable disable
    StreamGaugeSensorService service;
    
    public StreamGaugeSpec StreamGaugeSpec { get; private set; }
    public BlockObject BlockObject { get; private set; }
#nullable enable

    public StreamGaugeSensorMeasurement? WaterLevel { get; private set; }
    public StreamGaugeSensorVolumeMeasurement? VolumeMeasurement { get; set; }

    [Inject]
    public void Inject(StreamGaugeSensorService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        BlockObject = GetComponentFast<BlockObject>();
        StreamGaugeSpec = GetComponentFast<StreamGaugeSpec>();
        enabled = false;
    }

    public override void Tick()
    {
        if (!enabled)
        {
            Debug.Log("Tick called when not enabled");
            return;
        }

        WaterLevel = service.MeasureSensorLevel(this);
    }

    public void OnSelect()
    {
        if (BlockObject.IsFinished
            && service.CanUseProject)
        {
            enabled = true;
        }
    }

    public void OnUnselect()
    {
        WaterLevel = null;
        enabled = false;
    }

}
