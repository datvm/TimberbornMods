namespace GlobalInventory.Services;

public interface IGlobalGoodsProvider
{
    IEnumerable<GlobalGoodSpec> GetAdditionalGlobalGoods();
}
