namespace MoreBuildingRenovations.Renovations;

public abstract class HarvestSpeed(string id, ILoc t) : FarmingSpeedBase(id, t)
{
    protected override string BuffLocKey => "LV.MBR.HarvestSpeedBuff";
    protected override string[] FamilyIds { get; } = [nameof(HarvestSpeed1), nameof(HarvestSpeed2)];

    protected override float GetSpeedBonus(FarmhouseActionSpeed speed) => speed.HarvestSpeedBonus;
    protected override void SetSpeedBonus(FarmhouseActionSpeed speed, float bonus) => speed.SetHarvestSpeed(bonus);
    protected override void ClearSpeedBonus(FarmhouseActionSpeed speed) => speed.ClearHarvestSpeed();
}

[BindRenovation]
public class HarvestSpeed1(ILoc t) : HarvestSpeed(nameof(HarvestSpeed1), t);

[BindRenovation]
public class HarvestSpeed2(ILoc t) : HarvestSpeed(nameof(HarvestSpeed2), t);
