namespace ModdableTimberborn.Services;

[BindSingleton]
public class HourlyEventService(EventBus eb, IDayNightCycle dayNightCycle) : ITickableSingleton, ILoadableSingleton
{
    int prevHour;

    public event EventHandler<NewHourEventArgs>? OnNewHour;

    public void Load()
    {
        prevHour = (int)dayNightCycle.HoursPassedToday;
    }

    public void Tick()
    {
        var hour = (int)dayNightCycle.HoursPassedToday;
        if (prevHour == hour) { return; }

        var ev = new NewHourEventArgs(hour, dayNightCycle.PartialDayNumber, dayNightCycle.IsNighttime);
        OnNewHour?.Invoke(this, ev);
        eb.Post(ev);

        prevHour = hour;
    }

}

public record NewHourEventArgs(int Hour, float PartialDay, bool IsNightTime);