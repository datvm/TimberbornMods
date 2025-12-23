namespace ModdableWeathers.Components;

public class ModdableWaterStrengthModifier(ModdableWaterStrengthModifierService m) : BaseComponent, IAwakableComponent, IWaterStrengthModifier
{

    WaterSource waterSource = null!;

    public void Awake()
    {
        waterSource = GetComponent<WaterSource>();
        waterSource.AddWaterStrengthModifier(this);
    }

    public float GetStrengthModifier() => m.CurrentModifier;
}
