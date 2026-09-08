namespace ConfigurableFaction.Services;

public class ConfigurableFactionSpecService(
    ISpecService specService,
    FactionOptionsProvider factionOptionsProvider
) : FactionSpecService(specService), ILoadableSingleton
{
    public ImmutableArray<FactionSpec> OriginalFactions { get; private set; } = [];

    public new void Load()
    {
        base.Load();

        OriginalFactions = Factions;
        factionOptionsProvider.AddMissingFactions(Factions.Select(q => q.Id));

        AppendData();
    }

    public void AppendData()
    {
        ImmutableArray<string> materialGroups = [.. Factions
            .SelectMany(f => f.MaterialGroups)
            .Distinct()];

        Factions = [.. OriginalFactions.Select(f => AppendFactionData(f, materialGroups))];
    }

    FactionSpec AppendFactionData(FactionSpec spec, ImmutableArray<string> materialGroups)
    {
        var options = factionOptionsProvider.FactionOptions[spec.Id];

        var goods = spec.Goods.Concat(options.Goods).Distinct();
        var needs = spec.Needs.Concat(options.Needs).Distinct();

        return spec with
        {
            MaterialGroups = materialGroups,
            PrefabGroups = [.. spec.PrefabGroups, FactionOptionsProvider.GetPrefabGroupId(spec.Id)],
            Goods = [.. goods],
            Needs = [.. needs],
        };
    }

}
