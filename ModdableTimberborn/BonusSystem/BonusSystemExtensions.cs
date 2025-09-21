namespace ModdableTimberborn.BonusSystem;

public static class BonusSystemExtensions
{

    public static IReadOnlyDictionary<string, BonusTrackerItem> CurrentBonuses<T>(this T comp) 
        where T : IBonusTrackerComponent
        => comp.BonusTracker.CurrentBonuses;

    public static BonusTrackerChangeEventArgs AddOrUpdate<T>(this T comp, BonusTrackerItem item)
        where T : IBonusTrackerComponent
    {
        return comp.BonusTracker.AddOrUpdate(item);
    }

    public static BonusTrackerChangeEventArgs? Remove<T>(this T comp, string bonusId)
        where T : IBonusTrackerComponent
    {
        return comp.BonusTracker.Remove(bonusId);
    }

}
