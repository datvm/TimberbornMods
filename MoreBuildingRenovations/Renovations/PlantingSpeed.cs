namespace MoreBuildingRenovations.Renovations;

public abstract class PlantingSpeed(string id, ILoc t) : FarmingSpeedBase(id, t)
{
    protected override string BuffLocKey => "LV.MBR.PlantingSpeedBuff";
    protected override string[] FamilyIds { get; } = [nameof(PlantingSpeed1), nameof(PlantingSpeed2)];

    protected override float GetSpeedBonus(FarmhouseActionSpeed speed) => speed.PlantingSpeedBonus;
    protected override void SetSpeedBonus(FarmhouseActionSpeed speed, float bonus) => speed.SetPlantingSpeed(bonus);
    protected override void ClearSpeedBonus(FarmhouseActionSpeed speed) => speed.ClearPlantingSpeed();
}

[BindRenovation]
public class PlantingSpeed1(ILoc t) : PlantingSpeed(nameof(PlantingSpeed1), t);

[BindRenovation]
public class PlantingSpeed2(ILoc t) : PlantingSpeed(nameof(PlantingSpeed2), t);
