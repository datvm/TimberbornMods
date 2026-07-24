namespace MoreBuildingRenovations.Renovations;

[BindRenovation(AlsoBindSelf = true)]
public class ObservatoryPassive(NightlyScienceService service, TemplateNameMapper templateNameMapper) : RenovationBase
{
    static readonly ImmutableArray<string> TemplatePrefixes = ["Observatory."];
    static readonly string BonusId = "Renovation_" + nameof(ObservatoryPassive);

    public override string Id => nameof(ObservatoryPassive);
    public int SciencePerNightHour => (int)Spec.Parameters[0];

    public override bool IsAvailableToThisGame(RenovationRegistry registry) 
        => base.IsAvailableToThisGame(registry)
        // Only available if at least one observatory template exists in this game:
        && templateNameMapper._templates.Keys.AnyStartsWith(TemplatePrefixes); 

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.TemplateStartsWith(TemplatePrefixes);

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        building.GetComponent<BonusDescriptionComponent>().AddBonus(new(
            BonusId,
            Spec.Title.Value,
            Spec.Description ?? Spec.Title.Value
        ));

        service.ExtraSciencePerBuilding = SciencePerNightHour;
        service.Add(building);
    }
}
