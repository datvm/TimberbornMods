using SModel = ModdableTimberborn.BuildingSettings.BuiltInSettings.CachableStringSettingModel<Timberborn.Workshops.RecipeSpec>;

namespace ModdableTimberborn.BuildingSettings.BuiltInSettings;

public class ManufactorySettings(
    RecipeSpecService recipeSpecService,
    ILoc t
) : BuildingSettingsBase<Manufactory, SModel>(t)
{
    public override string DescribeModel(SModel model)
    {
        EnsureModelCached(model);
        return model.CachedDisplay!;
    }

    protected override bool ApplyModel(SModel model, Manufactory target)
    {
        var r = EnsureModelCached(model);

        var all = target.ProductionRecipes;
        if (target.CurrentRecipe == r || ((r is not null || all.Length <= 1) && !all.Contains(r)))
        {
            return false;
        }

        target.SetRecipe(r);
        return true;
    }

    RecipeSpec? EnsureModelCached(SModel model) => model.EnsureCached(t,
        s => recipeSpecService._recipeSpecs.GetOrDefault(s),
        GetRecipeName);

    string GetRecipeName(RecipeSpec r) => t.T(r.DisplayLocKey);

    protected override SModel GetModel(Manufactory duplicable)
    {
        var r = duplicable.CurrentRecipe;
        return new(r?.Id, r, t, GetRecipeName);
    }
}