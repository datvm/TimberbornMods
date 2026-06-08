namespace ModdableTimberborn.GameStats.Implementations;

public class GoodPercentStatsProvider(IGoodService goods, ResourceCountingService resourceCountingService) : IPercentGameStatProvider
{
    const string FillRatePrefix = "GoodFill.";

    public IEnumerable<string> AvailableStats => goods.Goods.Select(g => FillRatePrefix + g);

    public float GetStat(string statId)
    {
        if (statId .StartsWith(FillRatePrefix))
        {
            var stat = resourceCountingService.GetGlobalResourceCount(statId[FillRatePrefix.Length..]);
            return stat.FillRate;
        }

        throw new ArgumentOutOfRangeException();
    }
}
