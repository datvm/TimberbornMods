namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class NoAchForCyclesAchievement(
    ISingletonLoader loader,
    EventBus eb
) : EbAchievementBase(eb), ISaveableSingleton, ILoadableSingleton
{
    public const string AchId = "LV.MA.NoAchForCycles";

    static readonly SingletonKey SaveKey = new(nameof(NoAchForCyclesAchievement));
    static PropertyKey<int> CyclesKey => new(nameof(Cycles));

    public const int RequiredCycles = 20;

    public override string Id => AchId;

    public int Cycles { get; private set; }

    [OnEvent]
    public void OnCycleStarted(CycleStartedEvent _)
    {
        Cycles += 1;
        if (Cycles >= RequiredCycles)
        {
            Unlock();
        }
    }

    [OnEvent]
    public void OnAchievementUnlocked(ModdableAchievementUnlockedEvent e)
    {
        if (e.AchievementIds.Length > 0)
        {
            Cycles = 0;
        }
    }

    public override void EnableInternal()
    {
        if (Cycles >= RequiredCycles)
        {
            Unlock();
            return;
        }

        base.EnableInternal();
    }

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(CyclesKey))
        {
            Cycles = s.Get(CyclesKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        if (!IsEnabled) { return; }

        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(CyclesKey, Cycles);
    }

}
