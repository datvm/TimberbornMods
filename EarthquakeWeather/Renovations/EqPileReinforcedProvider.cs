namespace EarthquakeWeather.Renovations;

public class EqPileReinforcedProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "EqPileReinforced";
    public override string Id { get; } = RenoId;
    public override string? CanRenovate(BuildingRenovationComponent building) => ValidateActiveAndComponent<Stockpile>(building, "LV.EQ.EqPileReinforcedErr");
}
