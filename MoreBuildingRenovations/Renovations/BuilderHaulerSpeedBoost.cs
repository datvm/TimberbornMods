namespace MoreBuildingRenovations.Renovations;

[BindRenovation]
public class BuilderHaulerSpeedBoost(ILoc t) : RenovationBase
{
    public override string Id => nameof(BuilderHaulerSpeedBoost);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<HaulingCenterSpec>() || building.HasComponent<BuilderHubSpec>();

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        ImmutableArray<BonusSpec> bonuses = [
            BonusType.CarryingCapacity.ToBonusSpec(Spec.Parameters[0]),
            BonusType.MovementSpeed.ToBonusSpec(Spec.Parameters[1]),
        ];
        var desc = t.TWorkplaceWorkerBonus(bonuses);

        building.GetComponent<Components.WorkplaceBonuses>().AddBonus(new(
            bonuses,
            new(
                $"Renovation_{Id}",
                Spec.Title.Value,
                desc
            )
        ));
    }
}
