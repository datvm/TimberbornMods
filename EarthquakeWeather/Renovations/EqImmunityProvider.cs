namespace EarthquakeWeather.Renovations;

public class EqImmunityProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "EqImmunity";
    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building) => ValidateActive(building);

}
