namespace ModdableTimberborn.BonusSystem;

public class BonusTrackerItemSerializer : IValueSerializer<BonusTrackerItem>
{
    public static readonly BonusTrackerItemSerializer Instance = new();

    static readonly PropertyKey<string> ItemIdKey = new("Id");
    static readonly ListKey<string> BonusIdsKey = new("BonusIds");
    static readonly ListKey<float> ValueKey = new("Values");

    public Obsoletable<BonusTrackerItem> Deserialize(IValueLoader valueLoader)
    {
        var s = valueLoader.AsObject();

        var id = s.Get(ItemIdKey);
        var names = s.Get(BonusIdsKey);
        var values = s.Get(ValueKey);

        return new BonusTrackerItem(id, [..names.Zip(values, CommonExtensions.CreateBonusSpec)]);
    }

    public void Serialize(BonusTrackerItem value, IValueSaver valueSaver)
    {
        var s = valueSaver.AsObject();

        s.Set(ItemIdKey, value.Id);
        s.Set(BonusIdsKey, [..value.Bonuses.Select(q => q.Id)]);
        s.Set(ValueKey, [..value.Bonuses.Select(q => q.MultiplierDelta)]);
    }

}
