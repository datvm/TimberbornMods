
namespace MechanicalFilterPump.Components;

public class MechanicalFilterPumpComponent : BaseComponent, IPersistentEntity, IStartableComponent
{

    static readonly ComponentKey SaveKey = new("MechanicalFilterPumpComponent");
    static readonly PropertyKey<bool> ActiveKey = new("IsActive");
    static readonly PropertyKey<bool> NoPowerIncreaseKey = new("NoPowerIncrease");

    public bool IsActive { get; private set; }

#nullable disable
    MechanicalFilterPumpPower power;
#nullable enable

    public bool NoPowerIncrease { get; private set; }

    public void Start()
    {
        power = GetComponent<MechanicalFilterPumpPower>();
        SetPowerState();
    }

    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) { return; }

        IsActive = isActive;
        SetPowerState();
    }

    public void SetPowerCheat(bool enabled)
    {
        if (NoPowerIncrease == enabled) { return; }

        NoPowerIncrease = enabled;
        SetPowerState();
    }

    void SetPowerState()
    {
        var shouldModifyPower = IsActive && !NoPowerIncrease;
        power.Toggle(shouldModifyPower);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        IsActive = s.Has(ActiveKey) && s.Get(ActiveKey);
        NoPowerIncrease = s.Has(NoPowerIncreaseKey) && s.Get(NoPowerIncreaseKey);
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);

        if (IsActive)
        {
            s.Set(ActiveKey, IsActive);
        }

        if (NoPowerIncrease)
        {
            s.Set(NoPowerIncreaseKey, NoPowerIncrease);
        }
    }
}
