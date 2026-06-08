namespace ModdableTimberborn.GameStats.Implementations;

public class GoodStatsProvider(IGoodService goods, ResourceCountingService resourceCountingService) : IIntGameStatProvider
{
    const string GoodAmountPrefix = "GoodAmount.";
    const string CapacityPrefix = "GoodCapacity.";

    public IEnumerable<string> AvailableStats
    {
        get
        {
            foreach (var g in goods.Goods)
            {
                yield return GoodAmountPrefix + g;
                yield return CapacityPrefix + g;
            }
        }
    }

    public int GetStat(string statId)
    {        
        if (statId.StartsWith(GoodAmountPrefix))
        {
            var stat = GetCount(statId[GoodAmountPrefix.Length..]);
            return stat.AvailableStock;
        }

        if (statId.StartsWith(CapacityPrefix))
        {
            var stat = GetCount(statId[CapacityPrefix.Length..]);
            return stat.InputOutputCapacity;
        }

        throw new ArgumentOutOfRangeException();
    }

    ResourceCount GetCount(string goodId) => resourceCountingService.GetGlobalResourceCount(goodId);

}
