namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltCarrierService(BeltGoodService goods, BeltRegistry registry, ConveyorBeltSettings settings, ILoc t)
{
    public readonly ILoc t = t;

    public int Index(string speedId) => settings.Index(speedId);

    public float ItemsPerHour(int speed) => settings.ItemsPerHour(speed);

    public bool IsCarryable(string goodId) => goods.IsCarryable(goodId);

    public void Register(BeltCarrier carrier) => registry.Register(carrier);

    public void Unregister(BeltCarrier carrier) => registry.Unregister(carrier);

    public void Drop(Vector3Int cell, List<BeltGood> items) => goods.Drop(cell, items);

    public EntityDescription Describe(float itemsPerHour)
    {
        var lines = string.Join(Environment.NewLine, [
            $"{SpecialStrings.RowStarter} {t.T("LV.CBlt.Rate", itemsPerHour)}",
            $"{SpecialStrings.RowStarter} {t.T("LV.CBlt.CanTransferBuildings")}",
        ]);
        return EntityDescription.CreateTextSection(lines, 200);
    }
}
