namespace ConveyorBelt.Services;

[BindSingleton]
public class BeltInventoryService(BeltGoodService goods, BeltRegistry registry, ILoc t)
{
    public readonly ILoc t = t;

    public void Drop(Vector3Int cell, List<BeltGood> items) => goods.Drop(cell, items);

    public void Register(BeltInventory inventory) => registry.Register(inventory);

    public void Unregister(BeltInventory inventory) => registry.Unregister(inventory);
}
