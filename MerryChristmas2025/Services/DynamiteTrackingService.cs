namespace MerryChristmas2025.Services;

public class DynamiteTrackingService(
    EventBus eb,
    DistrictCenterRegistry districtCenterRegistry,
    RecoveredGoodStackSpawner stackSpawner
) : ILoadableSingleton
{
    readonly Dictionary<string, int> availableGoods = [];
    float goodScannedFrame = -1;

    static readonly bool IsDecember = DateTime.Now.Month == 12;
    const int NonDecemberChance = 5;

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        var spec = e.Entity.GetComponent<DynamiteSpec>();
        if (spec is null) { return; }

        if (!IsDecember && UnityEngine.Random.RandomRangeInt(0, 100) > NonDecemberChance) { return; }

        ScanForGoods();
        if (availableGoods.Count == 0) { return; }

        var blockObj = e.Entity.GetComponent<BlockObject>();
        var coord = blockObj.Coordinates;

        var itemCount = spec.Depth;
        var goods = new GoodAmount[itemCount];
        for (int i = 0; i < itemCount; i++)
        {
            var index = UnityEngine.Random.RandomRangeInt(0, availableGoods.Count);
            var goodId = availableGoods.Keys.ElementAt(index);
            var amount = UnityEngine.Random.RandomRangeInt(1, Math.Max(1, availableGoods[goodId] / 10));
            goods[i] = new(goodId, amount);
        }

        stackSpawner.AddAwaitingGoods(coord, goods);
    }

    void ScanForGoods()
    {
        if (Time.frameCount == goodScannedFrame) { return; }
        goodScannedFrame = Time.frameCount;

        availableGoods.Clear();

        foreach (var dc in districtCenterRegistry.FinishedDistrictCenters)
        {
            var inventories = dc.GetComponent<DistrictInventoryRegistry>();

            foreach (var inv in inventories.Inventories)
            {
                foreach (var g in inv.AllowedGoods)
                {
                    var id = g.StorableGood.GoodId;
                    var amount = inv.AmountInStock(id);

                    if (amount > 0)
                    {
                        availableGoods[id] = availableGoods.GetOrDefault(id) + amount;
                    }
                }
            }
        }
    }

}
