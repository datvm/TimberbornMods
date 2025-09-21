namespace ModdableTimberborn.BonusSystem;

public class BonusTracker(BonusManager bonusManager)
{

    public BonusManager BonusManager { get; } = bonusManager;

    public event EventHandler<BonusTrackerChangeEventArgs>? OnBonusChanged;

    readonly Dictionary<string, BonusTrackerItem> bonuses = [];
    public IReadOnlyDictionary<string, BonusTrackerItem> CurrentBonuses => bonuses;

    public BonusTrackerChangeEventArgs AddOrUpdate(BonusTrackerItem item)
    {
        var bonusId = item.Id;
        var oldBonuses = InternalRemoveBonus(bonusId);

        bonuses[bonusId] = item;
        ApplyBonuses(item.Bonuses);

        var args = new BonusTrackerChangeEventArgs(bonusId, oldBonuses, item);
        OnBonusChanged?.Invoke(this, args);
        return args;
    }

    public BonusTrackerChangeEventArgs? Remove(string bonusId)
    {
        var oldBonuses = InternalRemoveBonus(bonusId);
        if (oldBonuses is null) { return null; }

        var args = new BonusTrackerChangeEventArgs(bonusId, oldBonuses, null);
        OnBonusChanged?.Invoke(this, args);
        return args;
    }

    BonusTrackerItem? InternalRemoveBonus(string bonusId)
    {
        if (!bonuses.TryGetValue(bonusId, out var oldBonuses)) { return null; }

        bonuses.Remove(bonusId);
        BonusManager.RemoveBonuses(oldBonuses.Bonuses);

        return oldBonuses;
    }

    void ApplyBonuses(IReadOnlyList<BonusSpec> bonuses)
    {
        BonusManager.AddBonuses(bonuses);
    }

}

public readonly record struct BonusTrackerChangeEventArgs(string BonusId, BonusTrackerItem? OldValue, BonusTrackerItem? NewValue);