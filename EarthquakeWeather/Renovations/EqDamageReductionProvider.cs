namespace EarthquakeWeather.Renovations;

public class EqDamageReductionProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "EqDamageReduction";

    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building) => ValidateActive(building);
}
