
namespace StreamGaugeTracker.Components;

public class StreamGaugeTrackerComponent : TickableComponent, IPersistentEntity
{
    public static readonly PropertyKey<float> LowestDepthKey = new("LowestWaterLevel");
    public static readonly ListKey<Vector3> DepthHistoryKey = new("WaterLevelHistory");
    public static readonly PropertyKey<float> NextRecordTimeKey = new("NextRecordTime");

#nullable disable
    public StreamGauge StreamGauge { get; private set; }
    StreamGaugeTrackerService service;
    LimitedDataQueue depths;
#nullable enable

    public float LowestDepth { get; set; } = float.MaxValue;
    public float HighestHistoryDepth => Math.Max(DepthHistory.Highest, StreamGauge.HighestWaterLevel);
    public IReadOnlyLimitedDataQueue DepthHistory => depths;

    float nextRecordTime;

    [Inject]
    public void Inject(StreamGaugeTrackerService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        StreamGauge = GetComponentFast<StreamGauge>();
        depths ??= new(service.SamplingCount);
    }

    void ScheduleNextRecord() => nextRecordTime = service.ScheduleNextRecordTime();

    public override void Tick()
    {
        var depth = StreamGauge.WaterLevel;
        if (depth < LowestDepth)
        {
            LowestDepth = depth;
        }

        var time = service.DayNightCycle.PartialDayNumber;
        if (time >= nextRecordTime)
        {
            EnsureSize();
            depths.Add(new(StreamGauge.WaterLevel, time, service.IsHazardousWeather));
            ScheduleNextRecord();
        }
    }

    public void EnsureSize()
    {
        if (depths.MaxSize != service.SamplingCount)
        {
            depths.Resize(service.SamplingCount);
        }
    }

    public void ResetLowestValue() => LowestDepth = StreamGauge.WaterLevel;

    public void Load(IEntityLoader entityLoader)
    {
        depths = new(service.SamplingCount);

        if (!entityLoader.TryGetComponent(StreamGauge.StreamGaugeKey, out var s)) { return; }

        if (s.Has(LowestDepthKey))
        {
            LowestDepth = s.Get(LowestDepthKey);
        }

        if (s.Has(DepthHistoryKey))
        {
            depths.AddRange(s.Get(DepthHistoryKey)
                .Select(q => new StreamGaugeSample(q.x, q.y, q.z > 0f)));
        }

        if (s.Has(NextRecordTimeKey))
        {
            nextRecordTime = s.Get(NextRecordTimeKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(StreamGauge.StreamGaugeKey);
        s.Set(LowestDepthKey, LowestDepth);
        s.Set(DepthHistoryKey, [.. depths.Select(q => new Vector3(q.Depth, q.Time, q.IsHazardousWeather ? 1f : 0f))]);
        s.Set(NextRecordTimeKey, nextRecordTime);
    }

}
