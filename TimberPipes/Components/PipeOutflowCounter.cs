namespace TimberPipes.Components;

[AddTemplateModule2(typeof(OutflowCounterSpec))]
public class PipeOutflowCounter(IDayNightCycle dayNight) : BaseComponent, IOutflowCounter
{
    readonly OutflowHourWindow window = new();

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
}
