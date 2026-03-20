
namespace SkyblightWeather.Services;

[BindSingleton]
public partial class BadrainCharacterApplier(
    DefaultEntityTracker<BadrainCharacterComponent> tracker,
    IDayNightCycle dayNightCycle,
    EventBus eb
) : ILoadableSingleton
{
    bool active;

    public float BotDamagerPercent { get; private set; }
    public float? BotDamagePerTick { get; private set; }
    public (int Min, int Max, float Delta) BeaverSicknessValues { get; private set; }

    public void Load()
    {
        eb.Register(this);
    }

    public void StartBadrain(BadrainModifierSettings s)
    {
        if (!s.SickBeavers && !s.DamageBots) { return; }

        if (s.SickBeavers)
        {
            var min = Math.Max(0, dayNightCycle.HoursToTicks(s.SickBeaversMin));
            var max = dayNightCycle.HoursToTicks(s.SickBeaversMax);
            BeaverSicknessValues = (min, max, max - min);
        }

        if (s.DamageBots)
        {
            // s.BotDamage: Percent (0-100) of bot health to remove over 1 hour
            BotDamagerPercent = s.BotDamage / 100f;
            BotDamagePerTick = BotDamagerPercent / dayNightCycle.HoursToTicks(1);
        }

        StartTracking();

        active = true;
    }

    void StartTracking()
    {
        tracker.OnEntityRegistered += OnEntityRegistered;
        tracker.OnEntityUnregistered += OnEntityUnregistered;
        foreach (var entity in tracker.Entities)
        {
            OnEntityRegistered(entity);
        }
    }

    void StopTracking()
    {
        tracker.OnEntityRegistered -= OnEntityRegistered;
        tracker.OnEntityUnregistered -= OnEntityUnregistered;
        foreach (var entity in tracker.Entities)
        {
            OnEntityUnregistered(entity);
        }
    }

    void OnEntityRegistered(BadrainCharacterComponent obj)
    {
        obj.EnableComponent();
    }

    void OnEntityUnregistered(BadrainCharacterComponent obj)
    {
        obj.DisableComponent();
    }

    public void EndBadrain()
    {
        active = false;
        BeaverSicknessValues = default;
        BotDamagePerTick = null;
        StopTracking();
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        if (!active || tracker.Entities.Count == 0) { return; }

        var values = BeaverSicknessValues;
        if (values == default) { return; }

        foreach (var e in tracker.Entities)
        {
            e.AfflictBadRainSickness();
        }
    }

}
