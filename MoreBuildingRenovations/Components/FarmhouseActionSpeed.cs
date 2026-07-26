namespace MoreBuildingRenovations.Components;

/// <summary>
/// Holds Farmhouse-only planting/harvest action speed bonuses from renovations.
/// Actual speed application is done via patches that read these multipliers.
/// </summary>
[AddTemplateModule2(typeof(FarmHouse))]
public class FarmhouseActionSpeed : BaseComponent
{
    public float PlantingSpeedBonus { get; private set; }
    public float HarvestSpeedBonus { get; private set; }

    public float PlantingSpeedMultiplier => 1f + PlantingSpeedBonus;
    public float HarvestSpeedMultiplier => 1f + HarvestSpeedBonus;

    public bool HasPlantingSpeedBonus => PlantingSpeedBonus > 0f;
    public bool HasHarvestSpeedBonus => HarvestSpeedBonus > 0f;

    public void SetPlantingSpeed(float bonus) => PlantingSpeedBonus = bonus;

    public void ClearPlantingSpeed() => PlantingSpeedBonus = 0f;

    public void SetHarvestSpeed(float bonus) => HarvestSpeedBonus = bonus;

    public void ClearHarvestSpeed() => HarvestSpeedBonus = 0f;
}
