namespace ConfigurableTubeZipLine.Services;

public class TemplateCollectionSpecModifier(IAssetLoader assets) : BaseSpecModifier<TemplateCollectionSpec>
{

    protected override IEnumerable<NamedSpec<TemplateCollectionSpec>> Modify(IEnumerable<NamedSpec<TemplateCollectionSpec>> specs)
    {
        foreach (var namedSpec in specs)
        {
            switch (namedSpec.Spec.CollectionId)
            {
                case "Buildings.Folktails" when MSettings.TubewayForFolktails:
                    yield return AppendTemplate(namedSpec,
                    [
                        "Buildings/Paths/Tubeway/Tubeway.IronTeeth.blueprint",
                        "Buildings/Paths/TubewayStation/TubewayStation.IronTeeth.blueprint",
                        "Buildings/Paths/VerticalTubeway/VerticalTubeway.IronTeeth.blueprint",
                    ]);
                    break;
                case "Buildings.IronTeeth" when MSettings.ZiplineForIronTeeth:
                    yield return AppendTemplate(namedSpec,
                    [
                        "Buildings/Paths/ZiplineBeam/ZiplineBeam.Folktails.blueprint",
                        "Buildings/Paths/ZiplinePylon/ZiplinePylon.Folktails.blueprint",
                        "Buildings/Paths/ZiplineStation/ZiplineStation.Folktails.blueprint",
                    ]);
                    break;
                default:
                    yield return namedSpec;
                    break;
            }
        }
    }

    NamedSpec<TemplateCollectionSpec> AppendTemplate(in NamedSpec<TemplateCollectionSpec> original, IEnumerable<string> buildings)
    {
        HashSet<string> existingBuildings = [.. original.Spec.Blueprints.Select(q => q.Path)];
        var addingBuildings = buildings
            .Where(p => !existingBuildings.Contains(p))
            .Select(p => new AssetRef<BlueprintAsset>(p, new(() => assets.Load<BlueprintAsset>(p))));

        return original with
        {
            Spec = original.Spec with
            {
                Blueprints = [.. original.Spec.Blueprints, .. addingBuildings],
            }
        };
    }

}
