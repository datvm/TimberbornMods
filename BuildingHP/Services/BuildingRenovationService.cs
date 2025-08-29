namespace BuildingHP.Services;

public class BuildingRenovationService(
    BuildingHPRegistry registry,
    ITimeTriggerFactory timeTriggerFactory,
    IDayNightCycle dayNightCycle,
    RenovationRegistry renovationRegistry
) : ITickableSingleton
{

    public float PartialDayNumber => dayNightCycle.PartialDayNumber;
    public RenovationRegistry RenovationRegistry => renovationRegistry;

    public RenovationSpec GetSpec(string id) => RenovationRegistry.RenovationSpecService.Renovations[id];

    public void RegisterRenovation(BuildingRenovation r, Priority priority)
        => registry.RegisterRenovation(r, priority);

    public void CancelRenovation(BuildingRenovation r)
    {
        registry.UnregisterRenovation(r);
        r.PerformCancel();
    }

    public void FinishRenovation(BuildingRenovation r)
    {
        r.TimeTrigger?.Pause();
        registry.UnregisterRenovation(r);
        r.OnCompleted();
    }

    public void Tick()
    {
        ProcessMaterialQueue();
    }

    public ITimeTrigger CreateRenovationTrigger(BuildingRenovation r)
        => timeTriggerFactory.Create(() => FinishRenovation(r), r.Days ?? throw new ArgumentNullException(nameof(r.Days)));

    public ITimeTrigger? CreateRenovationTriggerIfNeeded(BuildingRenovation r) => r.Days is null ? null : CreateRenovationTrigger(r);

    public bool AcquireGoods(IEnumerable<GoodAmountSpecNew> goods, bool removeIfCanAcquire)
    {
        var sources = TryFindSources(goods);
        if (sources is null) { return false; }

        if (!removeIfCanAcquire) { return true; }
        RemoveStockpiles(sources);

        return true;
    }

    void ProcessMaterialQueue()
    {
        var queue = registry.RenovationQueue.PriorityDescSortedItems;
        if (queue.Count == 0) { return; }

        List<BuildingRenovation> removing = [];

        foreach (var r in queue)
        {
            if (r.IsGoodAcquired || r.IsDone) { continue; }

            if (AcquireGoods(r.Cost, true))
            {
                r.OnGoodAcquired(CreateRenovationTriggerIfNeeded(r));
                removing.Add(r);
            }
            else
            {
                r.OnGoodAcquireFailed();
            }
        }

        foreach (var r in removing)
        {
            registry.UnregisterRenovation(r);
        }

        registry.CleanupRenovationMaterialQueue();
    }

    void RemoveStockpiles(List<AcquireSource> sources)
    {
        foreach (var s in sources)
        {
            s.Building.Stockpile.Inventory.Take(s.Acquiring.ToGoodAmount());
        }
    }

    List<AcquireSource>? TryFindSources(IEnumerable<GoodAmountSpecNew> goods)
    {
        var remaining = goods
            .Where(q => q.Amount > 0)
            .ToDictionary(g => g.Id, g => g.Amount);
        if (remaining.Count == 0) { return []; }

        List<AcquireSource> sources = [];

        foreach (var sp in registry.Stockpiles.PriorityDescSortedItems)
        {
            var inv = sp.Stockpile.Inventory;
            foreach (var g in inv.UnreservedTakeableStock())
            {
                if (g.Amount == 0 || !remaining.TryGetValue(g.GoodId, out var needAmount) || needAmount <= 0) { continue; }

                var takeAmount = Math.Min(needAmount, g.Amount);
                sources.Add(new AcquireSource(sp, new()
                {
                    _amount = takeAmount,
                    _goodId = g.GoodId
                }));

                needAmount -= takeAmount;
                if (needAmount <= 0)
                {
                    remaining.Remove(g.GoodId);

                    if (remaining.Count == 0)
                    {
                        return sources;
                    }
                }
                else
                {
                    remaining[g.GoodId] = needAmount;
                }
            }
        }

        return null;
    }

    readonly record struct AcquireSource(BuildingRenovationStockpileComponent Building, GoodAmountSpec Acquiring);

}
