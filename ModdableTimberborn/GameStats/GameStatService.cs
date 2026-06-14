namespace ModdableTimberborn.GameStats;

public class GameStatService(IEnumerable<IGameStatProvider> providers) : ILoadableSingleton
{
    FrozenDictionary<string, IGameStatProvider> providerByType = null!; 

    public void Load()
    {
        Dictionary<string, IGameStatProvider> dict = [];

        foreach (var p in providers)
        {
            foreach (var stat in p.AvailableStats)
            {
                if (dict.TryGetValue(stat, out var existing))
                {
                    throw new Exception($"Duplicate stat provider for stat '{stat}': {existing.GetType().FullName} and {p.GetType().FullName}");
                }

                dict[stat] = p;
            }
        }

        providerByType = dict.ToFrozenDictionary();
    }

    public bool HasStat(string stat) => providerByType.ContainsKey(stat);
    public IGameStatProvider GetProvider(string stat) => providerByType.TryGetValue(stat, out var existing)
        ? existing
        : throw new Exception($"Stat '{stat}' does not exist.");
    public T GetProvider<T>(string stat) where T : IGameStatProvider => (T)GetProvider(stat);

    public object? GetStat(string stat) => GetProvider(stat).GetStat(stat);
    public string GetStatFormatted(string stat) => GetProvider(stat).GetStatFormatted(stat);
    public T? GetStat<T>(string stat) => (T?)GetProvider(stat).GetStat(stat);

    public bool TryGetStat<T>(string stat, out T? value)
    {
        value = default;

        if (!providerByType.TryGetValue(stat, out var provider))
        {
            return false;
        }

        if (provider.GetStat(stat) is not T statValue)
        {
            return false;
        }

        value = statValue;
        return true;
    }

}