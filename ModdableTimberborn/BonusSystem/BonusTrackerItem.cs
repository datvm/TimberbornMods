namespace ModdableTimberborn.BonusSystem;

public readonly record struct BonusTrackerItem(string Id, IReadOnlyList<BonusSpec> Bonuses)
{

    public BonusTrackerItem(string id, BonusSpec bonus)
        : this(id, [bonus]) { }

    public BonusTrackerItem(string id, string bonusId, float value)
        : this(id, [CommonExtensions.CreateBonusSpec(bonusId, value)]) { }
    public BonusTrackerItem(string id, BonusType bonusType, float value)
        : this(id, [CommonExtensions.CreateBonusSpec(bonusType.ToString(), value)]) { }

}
