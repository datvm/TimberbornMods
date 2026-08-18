namespace Crane.Services;

[BindSingleton]
public class CraneInventoryInitializer(
    IGoodService goodService,
    InventoryInitializerFactory inventoryInitializerFactory
) : IDedicatedDecoratorInitializer<CraneInventory, Inventory>
{
    public void Initialize(CraneInventory subject, Inventory decorator)
    {
        List<StorableGoodAmount> goods = [];
        foreach (var goodId in goodService.Goods)
        {
            goods.Add(new(StorableGood.CreateGiveableAndTakeable(goodId), int.MaxValue));
        }

        var initializer = inventoryInitializerFactory.Create(decorator, int.MaxValue, "Crane");
        initializer.AddAllowedGoods(goods);
        initializer.AddGoodDisallower(subject);
        initializer.HasPublicOutput();
        initializer.Initialize();
        subject.InitializeInventory(decorator);
    }

}
