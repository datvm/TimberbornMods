namespace MoreBuildingRenovations.Renovations;

[BindRenovation]
public class NumbercruncherSubmerge : RenovationBase
{
    static readonly string BonusId = "Renovation_" + nameof(NumbercruncherSubmerge);

    public override string Id => nameof(NumbercruncherSubmerge);

    public float ExtraScienceMul => Spec.Parameters[0];

    public override bool IsAvailableToThisGame(RenovationRegistry registry) 
        => registry.HasAnyBuildingWith<NumbercruncherRenovationSpec>() && base.IsAvailableToThisGame(registry);

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.HasComponent<NumbercruncherWaterLevel>();

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            BonusId,
            Spec.Title.Value,
            Spec.Description!
        ));

        building.GetComponent<NumbercruncherWaterLevel>().Activate(ExtraScienceMul);
    }
}
