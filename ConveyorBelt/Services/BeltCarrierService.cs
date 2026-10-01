namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltCarrierService(BeltGoodService goods, BeltRegistry registry, ConveyorBeltSpeeds speeds, FactionService factions, ILoc t)
{
    public readonly ILoc t = t;

    public float ItemsPerHour(string speedId) => speeds.ItemsPerHour(speedId);

    public bool IsCarryable(string goodId) => goods.IsCarryable(goodId);

    public void Register(BeltCarrier carrier) => registry.Register(carrier);

    public void Unregister(BeltCarrier carrier) => registry.Unregister(carrier);

    public void Drop(Vector3Int cell, List<BeltGood> items) => goods.Drop(cell, items);

    public EntityDescription Describe(string speedId, float itemsPerHour)
    {
        var tier = t.T(speeds.NameKey(speedId, factions.Current.Id));
        var line = t.T("LV.CBlt.TierRate", tier, itemsPerHour, BeltRates.Capacity);
        return EntityDescription.CreateTextSection($"{SpecialStrings.RowStarter} {line}", 200);
    }
}
