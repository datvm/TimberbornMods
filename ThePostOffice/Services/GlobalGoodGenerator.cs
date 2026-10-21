namespace ThePostOffice.Services;

[MultiBind(typeof(IGlobalGoodsProvider))]
public class GlobalGoodGenerator(IGoodService goods) : IGlobalGoodsProvider
{
    public IEnumerable<GlobalGoodSpec> GetAdditionalGlobalGoods()
    {
        foreach (var id in goods.Goods)
        {
            var goodSpec = goods.GetGood(id);
            yield return new()
            {
                Id = id,
                CopiedGoodId = id,
                Order = goodSpec.GoodOrder,
            };
        }
    }

}
