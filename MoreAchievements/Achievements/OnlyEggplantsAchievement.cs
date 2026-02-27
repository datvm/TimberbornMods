namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class OnlyEggplantsAchievement(
    EventBus eb,
    IGoodService goods,
    DefaultEntityTracker<Stockpile> stockpilesTracker
) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.OnlyEggplants";
    public const string GoodId = "EggplantRation";
    public const string FoodEffect = "Hunger";
    public const int AmountRequired = 250;

    public override string Id => AchId;
    readonly HashSet<string> foodIds = [];

    public bool HasEggplant { get; private set; }

    public override void EnableInternal()
    {
        InitializeList();

        if (!HasEggplant)
        {
            Disable();
            return;
        }

        base.EnableInternal();
    }

    void InitializeList()
    {
        HasEggplant = false;

        foodIds.Clear();
        foreach (var id in goods.Goods)
        {
            if (id == GoodId)
            {
                HasEggplant = true;
                continue;
            }
            
            var g = goods.GetGood(id);
            if (g.ConsumptionEffects.Any(e => e.NeedId == FoodEffect))
            {
                foodIds.Add(id);
            }
        }

        if (!HasEggplant)
        {
            foodIds.Clear();
        }
    }

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent _)
    {
        var total = 0;

        foreach (var (illegal, amount) in FindFoodStockpiles())
        {
            if (illegal is not null) { return; }
            total += amount;
        }

        if (total >= AmountRequired)
        {
            Unlock();
        }
    }

    public IEnumerable<(Stockpile? illegalStockpile, int amount)> FindFoodStockpiles()
    {
        foreach (var stockpile in stockpilesTracker.Entities)
        {
            foreach (var g in stockpile.Inventory.Stock)
            {
                if (g.Amount > 0 && foodIds.Contains(g.GoodId))
                {
                    yield return (stockpile, 0);
                }
                else if (g.GoodId == GoodId)
                {
                    yield return(null, g.Amount);
                }
            }
        }
    }

}
