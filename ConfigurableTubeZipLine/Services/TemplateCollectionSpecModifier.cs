namespace ConfigurableTubeZipLine.Services;

public class TemplateCollectionSpecModifier(AssetRefService assetRefService) : BaseSpecModifier<TemplateCollectionSpec>
{
    protected override IEnumerable<NamedSpec<TemplateCollectionSpec>> Modify(IEnumerable<NamedSpec<TemplateCollectionSpec>> specs)
    {
        foreach (var spec in specs)
        {
            yield return spec;
        }

        if (MSettings.TubewayForFolktails)
        {
            yield return new("TemplateCollection.Buildings.Folktails", new()
            {
                CollectionId = "Buildings.Folktails",
                Blueprints = [..assetRefService.CreateBlueprintAssetRefs([
                    "Buildings/Paths/Tubeway/Tubeway.IronTeeth.blueprint",
                    "Buildings/Paths/TubewayStation/TubewayStation.IronTeeth.blueprint",
                    "Buildings/Paths/VerticalTubeway/VerticalTubeway.IronTeeth.blueprint",
                ])],
            });
        }

        if (MSettings.ZiplineForIronTeeth)
        {
            yield return new("TemplateCollection.Buildings.IronTeeth", new()
            {
                CollectionId = "Buildings.IronTeeth",
                Blueprints = [..assetRefService.CreateBlueprintAssetRefs([
                    "Buildings/Paths/ZiplineBeam/ZiplineBeam.Folktails.blueprint",
                    "Buildings/Paths/ZiplinePylon/ZiplinePylon.Folktails.blueprint",
                    "Buildings/Paths/ZiplineStation/ZiplineStation.Folktails.blueprint",
                ])],
            });
        }
    }


}
