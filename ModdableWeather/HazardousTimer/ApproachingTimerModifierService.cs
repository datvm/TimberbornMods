namespace ModdableWeather.HazardousTimer;

public class ApproachingTimerModifierService(
    IEnumerable<IHazardousWeatherApproachingTimerModifier> modifiers,
    EventBus eb,
    ModdableWeatherService weatherService,
    ISpecService specs
) : ILoadableSingleton, IUnloadableSingleton
{

    static ApproachingTimerModifierService? instance;
    public static ApproachingTimerModifierService Instance => instance.InstanceOrThrow();

    public readonly ImmutableArray<IHazardousWeatherApproachingTimerModifier> Modifiers = [.. modifiers.OrderBy(q => q.Order)];

    public int DefaultDays { get; private set; } = -1;

    public int LastCycleWarned { get; private set; } = 0;

    public int ApproachingNotificationDays { get; private set; } = -1;

    public void Load()
    {
        instance = this;

        var spec = specs.GetSingleSpec<HazardousWeatherUISpec>();
        DefaultDays = spec.ApproachingNotificationDays;

        foreach (var m in Modifiers)
        {
            m.OnChanged += Recalculate;
        }
        Recalculate();
    }

    public void Unload() => instance = null;

    public void Recalculate()
    {
        var newValue = CalculateDays();
        if (newValue == ApproachingNotificationDays) { return; }

        ApproachingNotificationDays = newValue;
        RaiseWeatherChanged();
    }

    public int CalculateDays()
    {
        int def = DefaultDays,
            days = DefaultDays;
        if (days == -1) { return -1; }

        if (Modifiers.Length > 0)
        {
            foreach (var m in Modifiers)
            {
                if (!m.Disabled)
                {
                    days = m.Modify(days, def);
                }
            }
        }

        return days;
    }

    void RaiseWeatherChanged()
    {
        eb.Post(new OnModdableWeatherChangedMidCycle(weatherService.WeatherCycleDetails));
    }


}
