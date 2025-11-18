namespace AllGoodsInWarehouses.Services;

public class GoodSpecModifier : BaseSpecModifier<GoodSpec>
{
    public override int Order { get; } = 100;

    public const string WarehouseGoodType = "Box";

    public GoodSpec? Transform(GoodSpec spec)
    {
        if (spec.GoodType == WarehouseGoodType) { return null; }

        return spec with
        {
            GoodType = WarehouseGoodType,
            ContainerColor = spec.ContainerColor == "" ? "#FFFFFF" : spec.ContainerColor,
            StockpileVisualization = WarehouseGoodType,
        };
    }

    protected override IEnumerable<GoodSpec> Modify(IEnumerable<GoodSpec> specs)
    {
        foreach (var spec in specs)
        {
            var transformed = Transform(spec);
            yield return transformed is null ? spec : transformed;
        }
    }
}
