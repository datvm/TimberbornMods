namespace StreamGaugeTracker.Services;

public class StreamGaugeTrackerService(
    IDayNightCycle dayNightCycle,
    MSettings s,
    WeatherService weatherService
) : ILoadableSingleton
{

    public readonly IDayNightCycle DayNightCycle = dayNightCycle;

    public float RecordFrequencyDay { get; private set; }
    public int SamplingCount => s.SamplingCount.Value;
    public bool IsHazardousWeather => weatherService.IsHazardousWeather;

    public void Load()
    {
        s.SamplingFreq.ValueChanged += OnFreqChanged;
        OnFreqChanged(null!, 0);
    }

    void OnFreqChanged(object sender, int _)
    {
        RecordFrequencyDay = s.SamplingFreq.Value / (DayNightCycle.DaytimeLengthInHours + DayNightCycle.NighttimeLengthInHours);
    }

    public float ScheduleNextRecordTime() => DayNightCycle.PartialDayNumber + RecordFrequencyDay;

}
