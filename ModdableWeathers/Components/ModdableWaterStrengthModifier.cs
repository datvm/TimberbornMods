namespace ModdableWeathers.Components;

public class ModdableWaterStrengthModifier(ModdableWaterStrengthModifierService m) : BaseComponent, IAwakableComponent, IWaterStrengthModifier
{

    WaterSource waterSource = null!;
    public bool IgnoreDrought { get; set; }

    public void Awake()
    {
        waterSource = GetComponent<WaterSource>();
        waterSource.AddWaterStrengthModifier(this);
    }

    public float GetStrengthModifier() =>  IgnoreDrought ? m.CurrentModifierIgnoreDrought : m.CurrentModifier;
}

