
namespace AllGoodsInWarehouses.Services;

public class GoodSpecModifier : BaseSpecTransformer<GoodSpec>
{
    public const string WarehouseGoodType = "Box";

    public override GoodSpec? Transform(GoodSpec spec)
    {
        if (spec.GoodType == WarehouseGoodType) { return null; }

        return spec with
        {
            GoodType = WarehouseGoodType,
            ContainerColor = spec.ContainerColor with { a = 1, },
            StockpileVisualization = WarehouseGoodType,
        };
    }

}
