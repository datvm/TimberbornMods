namespace ModdableTimberborn.GameStats.Implementations;

public class GoodPercentStatsProvider(IGoodService goods, ResourceCountingService resourceCountingService) : IPercentGameStatProvider
{
    public IEnumerable<string> AvailableStats => goods.Goods.Select(GameStats.GoodFill);

    public float GetStat(string statId)
    {
        if (statId.StartsWith(GameStats.GoodFillPrefix))
        {
            var stat = resourceCountingService.GetGlobalResourceCount(statId[GameStats.GoodFillPrefix.Length..]);
            return stat.FillRate;
        }

        throw new ArgumentOutOfRangeException();
    }
}
