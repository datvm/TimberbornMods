namespace MoreBuildingRenovations.Renovations;

/// <summary>
/// Timed Farmhouse action-speed renovation. Parameters: [0]=speed bonus, [1]=duration days.
/// Family order in <see cref="FamilyIds"/> is the tier order: activating a higher index
/// deactivates every lower tier still active.
/// </summary>
public abstract class FarmingSpeedBase(string id, ILoc t) : ExpirableRenovationBase
{
    public override string Id => id;

    public float SpeedBonus => Spec.Parameters[0];

    public override float GetDurationDays(BuildingRenovationComponent building) => Spec.Parameters[1];

    protected abstract string BuffLocKey { get; }
    protected abstract string[] FamilyIds { get; }

    protected abstract float GetSpeedBonus(FarmhouseActionSpeed speed);
    protected abstract void SetSpeedBonus(FarmhouseActionSpeed speed, float bonus);
    protected abstract void ClearSpeedBonus(FarmhouseActionSpeed speed);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<FarmhouseActionSpeed>();

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => GetSpeedBonus(building.GetComponent<FarmhouseActionSpeed>()) >= SpeedBonus
            ? t.T("LV.BRe.AlreadyActive")
            : null;

    protected override void OnActivated(BuildingRenovationComponent building, bool isLoad)
    {
        DeactivateLowerTiers(building);

        var desc = t.TWorkplaceWorkerBonus(t.T(BuffLocKey, SpeedBonus));

        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            BonusId,
            Spec.Title.Value,
            desc,
            _ => building.Expirable.GetRemainingHoursOrNull(Id)
        ));

        SetSpeedBonus(building.GetComponent<FarmhouseActionSpeed>(), SpeedBonus);
    }

    public override void OnExpired(BuildingRenovationComponent building)
    {
        building.GetComponent<BonusDescriptionComponent>().RemoveBonus(BonusId);

        if (!FamilyIds.Any(building.HasActive))
        {
            ClearSpeedBonus(building.GetComponent<FarmhouseActionSpeed>());
        }

        base.OnExpired(building);
    }

    void DeactivateLowerTiers(BuildingRenovationComponent building)
    {
        var order = Array.IndexOf(FamilyIds, Id);
        if (order <= 0) { return; }

        for (var i = 0; i < order; i++)
        {
            var lowerId = FamilyIds[i];
            if (!building.HasActive(lowerId)) { continue; }

            building.Expirable.Cancel(lowerId);
            building.OnRenovationExpired(lowerId);
        }
    }

    string BonusId => "Renovation_" + Id;
}
