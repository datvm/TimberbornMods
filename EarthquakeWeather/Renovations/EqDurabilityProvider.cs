namespace EarthquakeWeather.Renovations;

public class EqDurabilityProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "EqDurability";
    public override string Id { get; } = RenoId;
    public override string? CanRenovate(BuildingRenovationComponent building)
    {
        var active = ValidateActive(building);
        if (active is not null) { return active; }

        var reinf = building.GetComponentFast<EarthquakeReinforcementComponent>();
        if (reinf.HasActiveSymbiosis)
        {
            return t.T("LV.BHP.AlreadyActive");
        }

        return null;
    }
}
