namespace ConfigurableFaction.Models;

public class BuildingDef(PlaceableBlockObjectSpec Placeable, Blueprint Blueprint, DataAggregatorService dataAggregator, ILoc t) : TemplateDefBase(Blueprint, dataAggregator, t)
{
    public PlaceableBlockObjectSpec PlaceableSpec { get; } = Placeable;
    public string GroupId => PlaceableSpec.ToolGroupId;

    public static BuildingDef? Create(Blueprint bp, DataAggregatorService dataAggregator, ILoc t)
        => bp.CreateDefinition<BuildingDef, PlaceableBlockObjectSpec>(comp => new(comp, bp, dataAggregator, t));

    protected override void InitializeRequirements(DataAggregatorService dataAggregator)
    {
        CheckForBuildingGoods(dataAggregator);
        CheckForBuildingNeeds();
    }

    void CheckForBuildingNeeds()
    {
        HashSet<string> needs = [];

        CheckForBuildingNeed<WorkshopRandomNeedApplierSpec>(spec => spec.Effects.Select(q => q.NeedId));
        CheckForBuildingNeed<ContinuousEffectBuildingSpec>(spec => spec.Effects.Select(q => q.NeedId));
        CheckForBuildingNeed<WonderEffectControllerSpec>(spec => spec.Effects.Select(q => q.NeedId));
        CheckForBuildingNeed<DwellingSpec>(spec => spec.SleepEffects.Select(q => q.NeedId));
        CheckForBuildingNeed<AttractionSpec>(spec => spec.Effects.Select(q => q.NeedId));
        CheckForBuildingNeed<AreaNeedApplierSpec>(spec => spec.Effects.Select(q => q.NeedId));

        // Add from goods
        needs.AddRange(RequiredGoods.SelectMany(g => g.RequiredNeeds));
        
        RequiredNeeds = [.. needs];

        void CheckForBuildingNeed<T>(Func<T, IEnumerable<string>> needsFn) where T : ComponentSpec
        {
            var comp = Blueprint.GetSpec<T>();
            if (comp is null) { return; }

            needs.UnionWith(needsFn(comp));
        }
    }

    void CheckForBuildingGoods(DataAggregatorService dataAggregator)
    {
        RequiredGoods = [
            ..CheckForManufactury(dataAggregator),
            ..CheckForBuildingCost(dataAggregator),
        ];
    }

    IEnumerable<GoodDef> CheckForManufactury(DataAggregatorService dataAggregator)
    {
        var manufacturer = Blueprint.GetSpec<ManufactorySpec>();
        if (manufacturer is null) { yield break; }

        var goods = dataAggregator.Goods.ItemsByIds;

        foreach (var recipeId in manufacturer.ProductionRecipeIds)
        {
            var recipe = dataAggregator.RecipesByIds[recipeId];

            foreach (var item in recipe.Ingredients)
            {
                yield return goods[item.Id];
            }

            foreach (var item in recipe.Products)
            {
                yield return goods[item.Id];
            }

            if (recipe.ConsumesFuel)
            {
                yield return goods[recipe.Fuel];
            }
        }
    }

    IEnumerable<GoodDef> CheckForBuildingCost(DataAggregatorService dataAggregator)
    {
        var spec = Blueprint.GetSpec<BuildingSpec>();
        if (spec is null)
        {
            yield break;
        }

        var goods = dataAggregator.Goods.ItemsByIds;
        foreach (var item in spec.BuildingCost)
        {
            yield return goods[item.Id];
        }
    }

}
