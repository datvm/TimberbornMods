namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeService(
    PostOfficeGlobalInventoryService inventory,
    PostOfficeConfigService configs,
    RecoveredGoodStackSpawner spawner
)
{
    public int ParcelWeight => configs.ParcelWeight;

    public void AddContribution(PostOfficeCapacity building)
    {
        inventory.AddCapacity(building.Contribution);
    }

    public void RemoveContribution(PostOfficeCapacity building)
    {
        var removing = Math.Min(building.Contribution, inventory.Capacity);
        var goods = inventory.GetGoodRemoval(removing).ToArray();
        if (goods.Length > 0)
        {
            inventory.Remove(goods);
            spawner.AddAwaitingGoods(building.Coordinates, goods);
        }

        inventory.RemoveCapacity(removing);
    }

    public int ParcelKg(float parcel) => Mathf.FloorToInt(parcel * ParcelWeight);
}
