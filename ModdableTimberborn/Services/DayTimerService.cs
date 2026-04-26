namespace ModdableTimberborn.Services;

public class DayTimerService(
    EventBus eb,
    ITimeTriggerFactory timeTriggerFactory
) : ILoadableSingleton
{
    public const float DefaultDelay = .25f;

    readonly Dictionary<float, List<DayTimerReference>> timers = [];

    public void Load()
    {
        eb.Register(this);
    }

    /// <summary>
    /// Registers an action to be triggered every day after the given in-game hour delay.
    /// </summary>
    /// <returns>A reference that can be used to unregister the timer.</returns>
    public DayTimerReference Register(Action action, float delayed = DefaultDelay) => Register(action, delayed, true);

    /// <summary>
    /// Registers an action to be triggered once on the next day after the given in-game hour delay.
    /// </summary>
    /// <returns>A reference that can be used to unregister the timer before it runs.</returns>
    public DayTimerReference RegisterOnce(Action action, float delayed = DefaultDelay) => Register(action, delayed, false);

    /// <summary>
    /// Registers an action to be triggered after the given in-game hour delay.
    /// </summary>
    /// <param name="repeat">Whether the timer repeats every day.</param>
    /// <returns>A reference that can be used to unregister the timer.</returns>
    public DayTimerReference Register(Action action, float delayed, bool repeat)
    {
        var reference = new DayTimerReference(delayed, action, repeat);
        timers.GetOrAdd(delayed, () => []).Add(reference);
        return reference;
    }

    /// <summary>
    /// Unregisters a previously registered timer.
    /// </summary>
    public void Unregister(DayTimerReference r)
    {
        if (timers.TryGetValue(r.DelayedHours, out var list))
        {
            list.Remove(r);

            if (list.Count == 0)
            {
                timers.Remove(r.DelayedHours);
            }
        }
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        if (timers.Count == 0) { return; }

        foreach (var delay in timers.Keys.ToArray())
        {
            timeTriggerFactory.CreateAndStart(() => TriggerEvents(delay), delay / 24f);
        }
    }

    void TriggerEvents(float delay)
    {
        if (!timers.TryGetValue(delay, out var list)) { return; }

        foreach (var r in list.ToArray())
        {
            r.Action();

            if (!r.Repeat)
            {
                list.Remove(r);
            }
        }

        if (list.Count == 0)
        {
            timers.Remove(delay);
        }
    }

}

public record DayTimerReference(float DelayedHours, Action Action, bool Repeat);
