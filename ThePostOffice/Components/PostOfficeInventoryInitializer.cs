namespace ThePostOffice.Components;

[BindSingleton]
public class PostOfficeInventoryInitializer(
    IGoodService goodService,
    InventoryInitializerFactory inventoryInitializerFactory
) : IDedicatedDecoratorInitializer<PostOfficeInventory, Inventory>
{
    public void Initialize(PostOfficeInventory subject, Inventory decorator)
    {
        List<StorableGoodAmount> goods = [];
        foreach (var goodId in goodService.Goods)
        {
            goods.Add(new(StorableGood.CreateGiveableAndTakeable(goodId), int.MaxValue));
        }

        var initializer = inventoryInitializerFactory.Create(decorator, int.MaxValue, "PostOffice");
        initializer.AddAllowedGoods(goods);
        initializer.AddGoodDisallower(subject);
        initializer.HasPublicOutput();
        initializer.Initialize();
        subject.InitializeInventory(decorator);
    }
}
