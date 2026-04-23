namespace Battery.Components;

[AddTemplateModule2(typeof(BatteryChargerSpec))]
public class BatteryCharger(BatteryChargingService service) : TickableComponent, IAwakableComponent, IPersistentEntity, IFinishedPausable
{
    static readonly ComponentKey SaveKey = new(nameof(BatteryCharger));
    static readonly PropertyKey<float> BufferKey = new(nameof(Buffer));

#nullable disable
    Stockpile stockpile;
    MechanicalBuilding mechanicalBuilding;
    MechanicalNode mechanicalNode;
    BlockableObject blockableObject;
#nullable enable

    public float Buffer { get; private set; }
    public ChargedBatterySpec? Charging { get; private set; }

    public void Awake()
    {
        stockpile = GetComponent<Stockpile>();
        mechanicalBuilding = GetComponent<MechanicalBuilding>();
        mechanicalNode = GetComponent<MechanicalNode>();
        blockableObject = GetComponent<BlockableObject>();

        blockableObject.ObjectBlocked += OnBlocked;
        blockableObject.ObjectUnblocked += OnUnblocked;
        if (!blockableObject.IsUnblocked)
        {
            DisableComponent();
        }
    }

    void OnBlocked(object sender, EventArgs e) => DisableComponent();

    void OnUnblocked(object sender, EventArgs e) => EnableComponent();

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        Buffer = s.Get(BufferKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(BufferKey, Buffer);
    }

    public override void Tick()
    {
        AppendBuffer();
        TryChargingBattery();
    }

    void AppendBuffer()
    {
        if (Buffer >= service.MaxBuffer)
        {
            mechanicalBuilding.SetConsumptionDisabled(true);
            return;
        }

        Buffer += mechanicalNode.Actuals.PowerInput * mechanicalNode.PowerEfficiency * service.HoursPerTick;

        if (Buffer > service.MaxBuffer)
        {
            Buffer = service.MaxBuffer;
        }
    }

    void TryChargingBattery()
    {
        var inv = stockpile.Inventory;

        var emptyBattery = GetEmptyBattery(inv);
        if (emptyBattery is null)
        {
            Charging = null;
            return;
        }

        var (id, spec) = emptyBattery.Value;
        Charging = spec;

        if (Buffer >= spec.RequiredCharges)
        {
            Buffer -= spec.RequiredCharges;
            inv.Take(new(id, 1));
            inv.GiveIgnoringCapacity(new(spec.ChargedGoodId, 1));

            mechanicalBuilding.SetConsumptionDisabled(false);
        }
    }

    (string Id, ChargedBatterySpec Spec)? GetEmptyBattery(Inventory inv)
    {
        foreach (var g in inv.UnreservedTakeableStock())
        {
            if (g.Amount > 0 && service.EmptyBatteries.TryGetValue(g.GoodId, out var spec))
            {
                return (g.GoodId, spec); // Typically with the limitation, there can only be one empty battery type
            }
        }

        return null;
    }

}
