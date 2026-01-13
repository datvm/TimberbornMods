namespace BlueprintRelics.Services;

public class BlueprintRelicCollectorService(
    IDayNightCycle dayNightCycle,
    ScienceService scienceService,
    EntityService entityService,
    IGoodService goodService,
    NotificationBus notfBus,
    ILoc t
) : ILoadableSingleton
{
    const float MinRequirementFactor = .5f;
    public readonly ILoc t = t;

    public float TicksInDay { get; private set; }

    public void Load()
    {
        TicksInDay = (dayNightCycle.DaytimeLengthInHours + dayNightCycle.NighttimeLengthInHours) / dayNightCycle.FixedDeltaTimeInHours;
    }

    public ImmutableArray<GoodAmount> GenerateGoodRequirements(BlueprintRelicSpec spec)
    {
        var goods = spec.PossibleRequiredGoods
            .Where(g => goodService.HasGood(g.Id))
            .ToArray();

        Dictionary<string, int> requiredGoods = [];
        for (int i = 0; i < spec.RequiredGoodsCount; i++)
        {
            var index = Random.Range(0, goods.Length);
            var good = goods[index];

            // Allow duplicates, just add to the amount:
            requiredGoods[good.Id] = requiredGoods.GetValueOrDefault(good.Id, 0) + RandomMinMax(good.Amount);
        }

        return [.. requiredGoods.Select(kv => new GoodAmount(kv.Key, kv.Value))];
    }

    public bool TryCollecting(BlueprintRelicCollector collector, DistrictCenter district)
    {
        if (collector.PauseCollecting
            || !district
            || scienceService.SciencePoints < collector.ScienceRequirement
            || !TryFindingGoods(district, collector.RequiredGoods, out var inventories)
        )
        {
            return false;
        }

        ConsumeGoods(inventories);
        scienceService.SubtractPoints(collector.ScienceRequirement);
        return true;
    }

    public void Expires(BlueprintRelicCollector collector)
    {
        notfBus.Post(t.T("LV.BRe.RelicExpired", collector.GetComponent<LabeledEntity>().DisplayName), collector);
        entityService.Delete(collector);
    }



    static bool TryFindingGoods(
        DistrictCenter districtCenter,
        ImmutableArray<GoodAmount> requirements,
        out List<KeyValuePair<Inventory, List<GoodAmount>>> inventories)
    {
        inventories = [];

        var remaining = requirements.ToDictionary(r => r.GoodId, r => r.Amount);
        var registry = districtCenter.GetComponent<DistrictInventoryRegistry>();

        foreach (var inv in registry.Inventories)
        {
            List<GoodAmount> found = [];

            foreach (var (id, amount) in remaining.ToArray())
            {
                var available = inv.UnreservedAmountInStock(id);
                if (available <= 0) { continue; }

                var toTake = Math.Min(available, amount);
                if (toTake >= amount)
                {
                    remaining.Remove(id);
                }
                else
                {
                    remaining[id] = amount - toTake;
                }

                found.Add(new GoodAmount(id, toTake));
            }

            if (found.Count > 0)
            {
                inventories.Add(new(inv, found));
            }
        }

        return remaining.Count == 0;
    }

    static void ConsumeGoods(List<KeyValuePair<Inventory, List<GoodAmount>>> inventories)
    {
        foreach (var (inv, goods) in inventories)
        {
            foreach (var g in goods)
            {
                inv.Take(g);
            }
        }
    }

    public static int RandomMinMax(int max) => Mathf.FloorToInt(Random.Range(MinRequirementFactor * max, max));

}
