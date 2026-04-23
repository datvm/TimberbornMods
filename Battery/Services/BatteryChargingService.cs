namespace Battery.Services;

[BindSingleton]
public class BatteryChargingService(
    IGoodService goods,
    IDayNightCycle dayNightCycle
) : ILoadableSingleton
{

    public float HoursPerTick { get; private set; }
    public int MaxBuffer { get; private set; }

    public FrozenDictionary<string, ChargedBatterySpec> EmptyBatteries { get; private set; } = FrozenDictionary<string, ChargedBatterySpec>.Empty;
    public FrozenDictionary<string, ChargedBatterySpec> ChargedBatteries { get; private set; } = FrozenDictionary<string, ChargedBatterySpec>.Empty;

    public void Load()
    {
        HoursPerTick = dayNightCycle.TicksToHours(1);

        Dictionary<string, ChargedBatterySpec> charges = [];
        Dictionary<string, ChargedBatterySpec> empties = [];
        MaxBuffer = 0;

        foreach (var id in goods.Goods)
        {
            var g = goods.GetGood(id);
            var chargeSpec = g.GetSpec<ChargedBatterySpec>();

            if (chargeSpec is null) { continue; }

            if (chargeSpec.Charges > 0)
            {
                charges[id] = chargeSpec;
            }
            else
            {
                if (chargeSpec.ChargedGoodId is null)
                {
                    throw new InvalidDataException($"ChargedBatterySpec for {id} has 0 charges but no ChargedGoodId");
                }
                else if (!goods.HasGood(chargeSpec.ChargedGoodId))
                {
                    throw new InvalidDataException($"ChargedBatterySpec for {id} has 0 charges but ChargedGoodId {chargeSpec.ChargedGoodId} does not exist");
                }
                else if (chargeSpec.ChargeEfficiency <= 0)
                {
                    throw new InvalidDataException($"ChargedBatterySpec for {id} has 0 charges but ChargeEfficiency is {chargeSpec.ChargeEfficiency}, must be > 0");
                }

                var chargedGood = goods.GetGood(chargeSpec.ChargedGoodId);
                var buffer = Mathf.CeilToInt(chargedGood.GetSpec<ChargedBatterySpec>().Charges / chargeSpec.ChargeEfficiency);
                chargeSpec.RequiredCharges = buffer;

                if (buffer > MaxBuffer)
                {
                    MaxBuffer = buffer;
                }

                empties.Add(id, chargeSpec);
            }
        }

        EmptyBatteries = empties.ToFrozenDictionary();
        ChargedBatteries = charges.ToFrozenDictionary();
    }

}
