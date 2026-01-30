
namespace ConfigurableFaction.Services.SpecAppenders;

[MultiBind(typeof(ISpecModifier))]
public class TemplateCollectionModifier(
    AssetRefService assetRefService,
    CurrentFactionSettingsProvider factionProvider
) : BaseCollectionModifier<TemplateCollectionSpec>(factionProvider)
{
    public static readonly FrozenDictionary<string, string> SpecialPairBuildings = new KeyValuePair<string, string>[]
    {
        new("Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth.blueprint", "Buildings/Monuments/EarthRepopulator/EarthRepopulator.IronTeeth.Plane.blueprint" ),
    }
        .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    static readonly ImmutableArray<string> ReservedCollections = ["Characters", "ModularShaftParts"];

    public static bool IsReserved(string id) => id.Split('.')
        .Any(p => ReservedCollections.Contains(p, StringComparer.OrdinalIgnoreCase));

    // Do not clear if it's special
    protected override TemplateCollectionSpec ClearCollection(TemplateCollectionSpec spec)
        => IsReserved(spec.CollectionId)
        ? spec
        : spec with { Blueprints = [], };

    protected override string GetId(TemplateCollectionSpec spec) => spec.CollectionId;

    protected override TemplateCollectionSpec ModifyModCollection(TemplateCollectionSpec spec)
        => spec with
        {
            Blueprints = [.. assetRefService.CreateBlueprintAssetRefs([
                .. YieldBuildingPaths(current.Buildings),
                .. current.Plants,
            ])],
        };

    static IEnumerable<string> YieldBuildingPaths(IEnumerable<string> paths)
    {
        foreach (var p in paths)
        {
            yield return p;

            if (SpecialPairBuildings.TryGetValue(p, out var pair))
            {
                yield return pair;
            }
        }
    }

}
