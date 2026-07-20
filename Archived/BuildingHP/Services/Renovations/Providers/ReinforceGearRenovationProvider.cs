namespace BuildingHP.Services.Renovations.Providers;

public class ReinforceGearRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    private const string GearId = "Gear";

    public const string RenovationId = "ReinforceGear";
    public override string Id { get; } = RenovationId;

    public override string? CanRenovate(BuildingRenovationComponent building)
    {
        var active = ValidateActive(building);
        if (active is not null) { return active; }

        var buildingSpec = building.GetComponentFast<BuildingSpec>();
        if (!buildingSpec) { goto NOT_AVAILABLE; } // Should not happen

        foreach (var c in buildingSpec.BuildingCost)
        {
            if (c.GoodId == GearId) { return null; }
        }

        var man = building.GetComponentFast<Manufactory>();
        if (!man) { goto NOT_AVAILABLE; }

        foreach (var r in man.ProductionRecipes)
        {
            foreach (var c in r.Ingredients)
            {
                if (c.Id == GearId) { return null; }
            }
        }

    NOT_AVAILABLE:
        return t.T("LV.BHP.NoGearBuilding");
    }

}
