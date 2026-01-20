namespace BlueprintRelics.Services;

[BindSingleton]
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
        Dictionary<int, List<GoodAmount>> requirementGroups = [];

        foreach (var group in spec.RequiredGoodGroups)
        {
            var list = requirementGroups.GetOrAdd(group.Index);
            var amount = group.MaxAmount;

            foreach (var good in group.GoodIds)
            {
                if (goodService.HasGood(good))
                {
                    list.Add(new GoodAmount(good, amount));
                }
            }
        }

        var groups = requirementGroups.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
        var result = new GoodAmount[groups.Length];

        for (int i = 0; i < result.Length; i++)
        {
            var goods = groups[i];

            var goodIndex = Random.RandomRangeInt(0, goods.Count);
            var good = goods[goodIndex];

            var amount = RandomMinMax(good.Amount);

            result[i] = new(good.GoodId, amount);
        }

        return [.. result];
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

    public static int RandomMinMax(int max) => Math.Max(1, Mathf.FloorToInt(Random.Range(MinRequirementFactor * max, max)));

}
