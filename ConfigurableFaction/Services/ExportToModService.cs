using TimberModBuilder.Services;

namespace ConfigurableFaction.Services;

public class ExportToModService(
    FactionOptionsProvider options,
    ConfigurableFactionSpecService factions
)
{
    public const string ModId = "ConfigurableFactionExported";
    public const string ModName = "Configurable Faction Exported";
    static readonly Timberborn.Versioning.Version Version = Timberborn.Versioning.Version.Create("0.7.0");

    public void ExportToMod()
    {
        var manifest = new ModManifest(
            ModName,
            "Exported data for Configurable Faction",
            Version,
            ModId,
            Version,
            [], []
        );

        var service = new TimberModBuilderService(manifest);
        AddPrefabGroups(service);
        AddFactionData(service);

        service.Build();
    }

    void AddPrefabGroups(TimberModBuilderService service)
    {
        foreach (var (facId, facOptions) in options.FactionOptions)
        {
            var id = FactionOptionsProvider.GetPrefabGroupId(facId);

            var paths = facOptions.Buildings
                .Concat(facOptions.Plantables)
                .Concat(facOptions.SpecialBuildings);

            service.AddBlueprints(new(
                id,
                nameof(PrefabGroupSpec),
                new()
                {
                    { nameof(PrefabGroupSpec.Id), id },
                    { nameof(PrefabGroupSpec.Paths), paths },
                }
            ));
        }
    }

    void AddFactionData(TimberModBuilderService service)
    {
        var allMaterialGroups = factions.Factions
            .SelectMany(q => q.MaterialGroups)
            .Distinct()
            .ToImmutableArray();

        foreach (var f in factions.OriginalFactions)
        {
            var facOpts = options.FactionOptions[f.Id];

            var materialGroups = allMaterialGroups.Except(f.MaterialGroups).Distinct();
            string[] prefabGroups = [FactionOptionsProvider.GetPrefabGroupId(f.Id)];
            var goods = facOpts.Goods.Except(f.Goods).Distinct();
            var needs = facOpts.Needs.Except(f.Needs).Distinct();

            service.AddBlueprints(new(
                f.Id,
                nameof(FactionSpec),
                new()
                {
                    { TimberModBuilderHelper.Append(nameof(FactionSpec.MaterialGroups)), materialGroups },
                    { TimberModBuilderHelper.Append(nameof(FactionSpec.PrefabGroups)), prefabGroups },
                    { TimberModBuilderHelper.Append(nameof(FactionSpec.Goods)), goods },
                    { TimberModBuilderHelper.Append(nameof(FactionSpec.Needs)), needs },
                }
            ));
        }
    }

}
