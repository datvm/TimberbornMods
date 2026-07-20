
namespace BeavlineLogistics.Renovations;

public class BeavlineBalancerRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    public const string RenoId = "BeavlineBalancer";

    public override string Id { get; } = RenoId;

    public override string? CanRenovate(BuildingRenovationComponent building)
        => ValidateActiveAndComponent<StockpileSpec>(building, "LV.BL.ErrStockpileOnly");

}
