namespace BuildingHP.Services.Renovations.Providers;

public class ProductionOverdriveRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "ProductionOverdrive";

    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building)
        => ValidateActiveAndComponent<Workplace>(building, "LV.BHP.NotWorkplace");
}
