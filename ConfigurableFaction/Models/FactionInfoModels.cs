namespace ConfigurableFaction.Models;

public class FactionsInfo
{

    public ImmutableArray<FactionInfo> Factions { get; init; }

}

public record FactionInfo(
    FactionSpec Spec,
    ImmutableArray<NeedSpec> Needs,
    ImmutableArray<GoodSpec> Goods,
    ImmutableArray<PrefabGroupSpec> PrefabGroupSpecs,
    ImmutableArray<NormalizedPrefabSpec> Buildings,
    ImmutableArray<PlantableGroup> Plantables,
    ImmutableArray<BuildingToolGroup> BuildingByToolGroups
)
{

    public ImmutableHashSet<string> NormalizedPrefabs { get; } = [
        .. Buildings.Select(q => q.NormalizedName), 
        .. Plantables.SelectMany(q => q.Plants).Select(q => q.NormalizedName)];

}

public readonly record struct BuildingToolGroup(ToolGroupSpec ToolGroupSpec, ImmutableArray<NormalizedPrefabSpec> Prefabs);
public readonly record struct PlantableGroup(string Group, ImmutableArray<NormalizedPrefabSpec> Plants);

public readonly record struct NormalizedPrefabSpec(string NormalizedName, string Path, PrefabSpec PrefabSpec)
{

    public static NormalizedPrefabSpec Create(PrefabSpec spec, string path, string faction)
    {
        var normalizedName = spec.PrefabName;

        if (normalizedName.EndsWith('.' + faction))
        {
            normalizedName = normalizedName[..^(faction.Length - 1)]; // Remove the faction suffix
        }

        return new NormalizedPrefabSpec(normalizedName, path, spec);
    }
}