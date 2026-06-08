namespace ModdableTimberborn.GameStats.Implementations;

public class GoodStatsProvider(IGoodService goods, ResourceCountingService resourceCountingService) : IIntGameStatProvider
{
    public IEnumerable<string> AvailableStats
    {
        get
        {
            foreach (var g in goods.Goods)
            {
                yield return GameStats.GoodAmount(g);
                yield return GameStats.GoodCapacity(g);
            }
        }
    }

    public int GetStat(string statId)
    {        
        if (statId.StartsWith(GameStats.GoodAmountPrefix))
        {
            var stat = GetCount(statId[GameStats.GoodAmountPrefix.Length..]);
            return stat.AvailableStock;
        }

        if (statId.StartsWith(GameStats.GoodCapacityPrefix))
        {
            var stat = GetCount(statId[GameStats.GoodCapacityPrefix.Length..]);
            return stat.InputOutputCapacity;
        }

        throw new ArgumentOutOfRangeException();
    }

    ResourceCount GetCount(string goodId) => resourceCountingService.GetGlobalResourceCount(goodId);

}
