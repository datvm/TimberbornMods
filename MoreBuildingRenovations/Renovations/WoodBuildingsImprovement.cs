namespace MoreBuildingRenovations.Renovations;

public abstract class WoodBuildingsImprovement(string id, ILoc t, string? requiredId = null) : RenovationBase
{
    static readonly ImmutableArray<string> TemplatePrefixes = ["LumberMill.", "IndustrialLumberMill.", "GearWorkshop.", "WoodWorkshop."];

    public override string Id => id;

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.TemplateStartsWith(TemplatePrefixes);

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => building.Service.GetRequiredRenovationIdReason(building, requiredId);

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        ImmutableArray<BonusSpec> bonuses = [ 
            BonusType.WorkingSpeed.ToBonusSpec(Spec.Parameters[0])
        ];
        var desc = t.TWorkplaceWorkerBonus(bonuses);

        building.GetComponent<Components.WorkplaceBonuses>().AddBonus(new(
            [BonusType.WorkingSpeed.ToBonusSpec(Spec.Parameters[0])],
            new(
                $"Renovation_{Id}",
                Spec.Title.Value,
                desc
            )
        ));
    }
}

[BindRenovation]
public class WoodBuildingsImprovement1(ILoc t) : WoodBuildingsImprovement(nameof(WoodBuildingsImprovement1), t);

[BindRenovation]
public class WoodBuildingsImprovement2(ILoc t) : WoodBuildingsImprovement(nameof(WoodBuildingsImprovement2), t, nameof(WoodBuildingsImprovement1));

[BindRenovation]
public class WoodBuildingsImprovement3(ILoc t) : WoodBuildingsImprovement(nameof(WoodBuildingsImprovement3), t, nameof(WoodBuildingsImprovement2));