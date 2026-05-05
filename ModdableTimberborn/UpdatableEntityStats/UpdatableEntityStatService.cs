namespace ModdableTimberborn.UpdatableEntityStats;

public class UpdatableEntityStatService
{

    readonly FrozenDictionary<string, IUpdatableEntityStat> stats;
    public readonly ImmutableArray<IUpdatableEntityStat> AllStats;
    public readonly ImmutableArray<IImageStat> AllImageStats;
    public readonly ImmutableArray<IPercentStat> AllPercentStats;

    public PopulationStat PopulationStat { get; }
    
    public UpdatableEntityStatService(IEnumerable<IUpdatableEntityStat> stats)
    {
        AllStats = [.. stats.OrderBy(t => t.Id)];
        this.stats = AllStats.ToFrozenDictionary(t => t.Id);        
        AllImageStats = [.. AllStats.OfType<IImageStat>()];
        AllPercentStats = [.. AllStats.OfType<IPercentStat>()];
        PopulationStat = (PopulationStat)this.stats[PopulationStat.StatId];
    }

    public bool TryGetStat(string id, [NotNullWhen(true)] out IUpdatableEntityStat? tracker)
        => stats.TryGetValue(id, out tracker);

    

}