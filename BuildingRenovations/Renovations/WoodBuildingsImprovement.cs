namespace BuildingRenovations.Renovations;

public abstract class WoodBuildingsImprovement(ILoc t) : RenovationBase
{
    static readonly ImmutableArray<string> TemplatePrefixes = ["LumberMill.", "IndustrialLumberMill.", "GearWorkshop.", "WoodWorkshop."];

    protected abstract string? RequiredId { get; }

    public override bool CanRenovate(BuildingRenovationComponent building)
        => building.TemplateStartsWith(TemplatePrefixes);

    public override string? GetUnavailableReason(BuildingRenovationComponent building)
        => building.Service.GetRequiredRenovationIdReason(building, RequiredId);

    public override void OnCompleted(BuildingRenovationComponent building, bool isLoad)
    {
        ImmutableArray<BonusSpec> bonuses = [
            BonusType.WorkingSpeed.ToBonusSpec(Spec.Parameters[0])
        ];
        var desc = t.TWorkplaceWorkerBonus(bonuses);

        building.GetWorkplaceBonusComponent().AddBonus(new(
            $"Renovation_{Id}", Spec.Title.Value,
            _ => desc,
            null,
            bonuses
        ));
    }
}

[BindRenovation]
public class WoodBuildingsImprovement1(ILoc t) : WoodBuildingsImprovement(t)
{
    public override string Id => nameof(WoodBuildingsImprovement1);
    protected override string? RequiredId => null;
}

[BindRenovation]
public class WoodBuildingsImprovement2(ILoc t) : WoodBuildingsImprovement(t)
{
    public override string Id => nameof(WoodBuildingsImprovement2);
    protected override string? RequiredId => nameof(WoodBuildingsImprovement1);
}

[BindRenovation]
public class WoodBuildingsImprovement3(ILoc t) : WoodBuildingsImprovement(t)
{
    public override string Id => nameof(WoodBuildingsImprovement3);
    protected override string? RequiredId => nameof(WoodBuildingsImprovement2);
}
