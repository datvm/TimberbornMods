namespace ThePostOffice.Services;

[BindSingleton]
public class PostOfficeService(
    PostOfficeGlobalInventoryService inventory,
    PostOfficeConfigService configs,
    RecoveredGoodStackSpawner spawner
)
{

    public int ParcelWeight => configs.ParcelWeight;

    public void OnFinished(PostOfficeComponent comp)
    {
        inventory.AddCapacity(comp.PostOfficeSpec.GlobalCapacityContribution);
    }

    public void OnExitFinished(PostOfficeComponent comp)
    {
        var removingCap = comp.PostOfficeSpec.GlobalCapacityContribution;
        // Remove until capacity
        var goods = inventory.GetGoodRemoval(removingCap).ToArray();
        if (goods.Length > 0)
        {
            SpawnRemovalGoods(comp, goods);
        }

        // Remove capacity
        inventory.RemoveCapacity(removingCap);
    }

    void SpawnRemovalGoods(PostOfficeComponent comp, IReadOnlyList<GoodAmount> goods)
    {
        inventory.Remove(goods);
        var coord = comp.BlockObject.Coordinates;
        spawner.AddAwaitingGoods(coord, goods);
    }

    public int ParcelKg(float parcel) => Mathf.FloorToInt(parcel * ParcelWeight);

}
