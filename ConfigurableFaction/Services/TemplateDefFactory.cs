namespace ConfigurableFaction.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu)]
public class TemplateDefFactory(ISpecService specs, ILoc t)
{

    public TemplateDefBase? Create(AssetRef<BlueprintAsset> bpRef, DataAggregatorService aggregator)
    {
        var path = bpRef.Asset.Path;
        var bp = specs.GetBlueprint(path);

        var placeable = bp.GetSpec<PlaceableBlockObjectSpec>();
        if (placeable is not null)
        {
            return new BuildingDef(placeable, bp, path, aggregator, t);
        }

        var plantable = bp.GetSpec<PlantableSpec>();
        if (plantable is not null)
        {
            return new PlantDef(plantable, bp, path, aggregator, t);
        }

        return null;
    }

}
