namespace HydroFormaProjects.Components;

public class StreamGaugeSensor(StreamGaugeSensorService service) : TickableComponent, ISelectionListener, IAwakableComponent
{

#nullable disable    
    public StreamGaugeSpec StreamGaugeSpec { get; private set; }
    public BlockObject BlockObject { get; private set; }
#nullable enable

    public StreamGaugeSensorMeasurement? WaterLevel { get; private set; }
    public StreamGaugeSensorVolumeMeasurement? VolumeMeasurement { get; set; }

    public void Awake()
    {
        BlockObject = GetComponent<BlockObject>();
        StreamGaugeSpec = GetComponent<StreamGaugeSpec>();
        DisableComponent();
    }

    public override void Tick()
    {
        WaterLevel = service.MeasureSensorLevel(this);
    }

    public void OnSelect()
    {
        if (BlockObject.IsFinished
            && service.IsUnlocked)
        {
            EnableComponent();
            Tick();
        }
    }

    public void OnUnselect()
    {
        WaterLevel = null;
        DisableComponent();
    }

}
