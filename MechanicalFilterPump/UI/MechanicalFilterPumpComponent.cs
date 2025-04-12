global using Timberborn.MechanicalSystem;

namespace MechanicalFilterPump.UI;

public class MechanicalFilterPumpComponent : BaseComponent, IPersistentEntity
{
    const float PowerMultiplier = 2f;

    static readonly ComponentKey SaveKey = new("MechanicalFilterPumpComponent");
    static readonly PropertyKey<bool> ActiveKey = new("IsActive");
    static readonly PropertyKey<bool> NoPowerIncreaseKey = new("NoPowerIncrease");

    public bool IsActive { get; private set; }
    public bool NoPowerIncrease { get; private set; }

    MechanicalNode mechanicalNode = null!;
    public int OriginalPowerInput { get; private set; }

    public int PowerIncrease => Mathf.CeilToInt(OriginalPowerInput * PowerMultiplier) - OriginalPowerInput;

    public void Awake()
    {
        mechanicalNode = GetComponentFast<MechanicalNode>();
    }

    public void Start()
    {
        OriginalPowerInput = mechanicalNode._nominalPowerInput;
    }

    public void SetActive(bool isActive)
    {
        if (IsActive == isActive) { return; }

        IsActive = isActive;
        SetPowerMultiplier();
    }

    public void SetPowerCheat(bool enabled)
    {
        if (NoPowerIncrease == enabled) { return; }

        NoPowerIncrease = enabled;        
        SetPowerMultiplier();
    }

    void SetPowerMultiplier(float? multiplier = null)
    {
        multiplier ??= IsActive && !NoPowerIncrease ? PowerMultiplier : 1f;

        mechanicalNode._nominalPowerInput = Mathf.CeilToInt(OriginalPowerInput * multiplier.Value);
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
