namespace ScienceShop.Services;

[BindSingleton]
public class ConsumeScienceRecipeSpecService(RecipeSpecService recipes) : ILoadableSingleton
{
    FrozenDictionary<string, ConsumeScienceRecipeSpec> byRecipeId =
        FrozenDictionary<string, ConsumeScienceRecipeSpec>.Empty;

    public void Load()
    {
        Dictionary<string, ConsumeScienceRecipeSpec> map = [];
        foreach (var recipe in recipes.GetRecipes())
        {
            var spec = recipe.GetSpec<ConsumeScienceRecipeSpec>();
            if (spec is null)
            {
                continue;
            }

            if (spec.ScienceCost <= 0)
            {
                throw new InvalidOperationException(
                    $"Recipe '{recipe.Id}' ConsumeScienceRecipeSpec.ScienceCost must be > 0.");
            }

            map[recipe.Id] = spec;
        }

        byRecipeId = map.ToFrozenDictionary();
    }

    public bool TryGet(RecipeSpec? recipe, [NotNullWhen(true)] out ConsumeScienceRecipeSpec? spec)
    {
        spec = null;
        return recipe is not null && byRecipeId.TryGetValue(recipe.Id, out spec);
    }
}
