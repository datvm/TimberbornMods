global using Timberborn.WaterBuildings;

namespace StreamGaugeVolume;

public class StreamGaugeVolumeComponent : BaseComponent
{
    static readonly FieldInfo CoordinatesField = typeof(StreamGauge).GetField("_coordinates", BindingFlags.NonPublic | BindingFlags.Instance);

    public StreamGaugeVolume? Volume { get; private set; }
    StreamGaugeVolumeService? service;

    [Inject]
    public void InjectDependencies(StreamGaugeVolumeService service)
    {
        this.service = service;
    }

    public void Measure()
    {
        Volume = null;

        var gauge = GetComponentFast<StreamGauge>();
        if (gauge is null) { return; }

        var coor = (Vector3Int)CoordinatesField.GetValue(gauge);
        Volume = service?.CalculateVolume(coor);
    }

}
