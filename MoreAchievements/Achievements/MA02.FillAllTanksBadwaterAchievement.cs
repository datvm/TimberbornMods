namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class FillAllTanksBadwaterAchievement(DefaultEntityTracker<Stockpile> stockpileTracker, EventBus eb) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.FillAllTanksBadwater";
    public override string Id => AchId;
    public const string BadwaterId = "Badwater";
    public const string GoodType = "Liquid";

    public const int MinimumBadwater = 1000;

    [OnEvent]
    public void OnNewDay(CycleDayStartedEvent e)
    {
        var (illegalTank, count) = ValidateFulfillment();

        if (!illegalTank && count >= MinimumBadwater)
        {
            Unlock();
        }
    }

    public (Stockpile? IllegalTank, int TotalCount) ValidateFulfillment()
    {
        var count = 0;

        foreach (var (stockpile, legal, amount) in EnumerateStockpile())
        {
            if (!legal) { return (stockpile, 0); }
            count += amount;
        }

        return (null, count);
    }

    IEnumerable<(Stockpile stockpile, bool legal, int amount)> EnumerateStockpile()
    {
        foreach (var stockpile in stockpileTracker.Entities)
        {
            // Not finished or not tank
            if (!stockpile 
                || !stockpile.GetComponent<BlockObject>().IsFinished
                || stockpile.WhitelistedGoodType != GoodType) { continue; }

            var inv = stockpile.Inventory;
            var stock = inv.Stock;

            if (stock.Count == 0)
            {
                yield return (stockpile, false, 0);
                continue;
            }

            foreach (var g in stock)
            {
                if ((g.GoodId != BadwaterId)
                    || (g.Amount < inv.Capacity))
                {
                    yield return (stockpile, false, 0);
                    continue;
                }

                yield return (stockpile, true, g.Amount);
            }
        }
    }

}
