namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class StackAllFoodWarehousesAchievement(
    EventBus eb,
    IGoodService goods,
    BuildingStackService buildingStackService
) : EbAchievementBase(eb)
{
    public const string AchId = "LV.MA.StackAllFoodWarehouses";
    const string FoodNeedId = "Hunger";

    public override string Id => AchId;

    readonly List<string> requiredGoods = [];
    public IEnumerable<string> RequiredGoodNames => requiredGoods.Select(id => goods.GetGood(id).DisplayName.Value);

    public override void EnableInternal()
    {
        base.EnableInternal();
        ListRequiredGoods();
    }

    void ListRequiredGoods()
    {
        var ids = goods.Goods;
        requiredGoods.Clear();
        foreach (var id in ids)
        {
            var g = goods.GetGoodOrNull(id);

            if (g is null || !g.HasConsumptionEffects ||
                !g.ConsumptionEffects.FastAny(e => e.NeedId == FoodNeedId)) { continue; }

            requiredGoods.Add(id);
        }
    }

    public override void DisableInternal()
    {
        base.DisableInternal();
        requiredGoods.Clear();
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        var stacks = buildingStackService.ScanForStackBuildings(IsStockpile, IsFullFoodStockpile);

        foreach (var stack in stacks)
        {
            if (stack.Count < requiredGoods.Count) { continue; }

            HashSet<string> goodsInStack = [];
            foreach (var sp in stack)
            {
                var goodId = ((SingleGoodAllower) sp.GetComponent<Stockpile>().Inventory._goodDisallower).AllowedGood;
                goodsInStack.Add(goodId);

                if (goodsInStack.Count == requiredGoods.Count)
                {
                    Unlock();
                    return;
                }
            }
        }
    }

    static bool IsStockpile(BlockObject obj) => obj.HasComponent<Stockpile>();

    bool IsFullFoodStockpile(BlockObject obj)
    {
        var sp = obj.GetComponent<Stockpile>();
        var inv = sp.Inventory;
        var goodId = ((SingleGoodAllower) inv._goodDisallower).AllowedGood;
        if (goodId is null || !requiredGoods.Contains(goodId)) { return false; }

        return inv.LimitedAmount(goodId) == inv.AmountInStock(goodId);
    }

}
