namespace ThePostOffice.Components;

[BindSingleton]
public class PostOfficeInventoryInitializer(
    IGoodService goodService,
    InventoryInitializerFactory inventoryInitializerFactory
) : IDedicatedDecoratorInitializer<PostOfficeInventory, Inventory>
{
    public void Initialize(PostOfficeInventory subject, Inventory decorator)
    {
        var output = subject.GetComponent<PostOfficeReceiver>();
        List<StorableGoodAmount> goods = [];
        foreach (var id in goodService.Goods)
        {
            var storable = output
                ? StorableGood.CreateAsTakeable(id)
                : StorableGood.CreateAsGivable(id);
            goods.Add(new(storable, int.MaxValue));
        }

        var initializer = inventoryInitializerFactory.Create(decorator, int.MaxValue, "PostOffice");
        initializer.AddAllowedGoods(goods);
        initializer.AddGoodDisallower(subject);
        if (output)
        {
            initializer.HasPublicOutput();
        }

        initializer.Initialize();
        subject.InitializeInventory(decorator);
    }
}
