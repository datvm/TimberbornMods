namespace BlueprintRelics.Services;

[BindSingleton]
public class BlueprintRelicRecipeService(
    BlueprintRelicRecipeRegistry recipeRegistry,
    ModdableRecipeLockService locker,
    ModdableRecipePersistentUnlocker persistentUnlocker
)
{

    public List<BlueprintRelicRecipePair>[] GetLockedRecipesByRarity() => [
        .. recipeRegistry.RecipesByRarity
            .Select(recipes => recipes
                .Where(r => locker.IsLocked(r.Id, out _))
                .ToList())
    ];

    public IReadOnlyList<BlueprintRelicRecipePair> GetUnlockedRecipes() => [.. recipeRegistry.RecipesByRarity
        .SelectMany(r => r)
        .Where(r => !locker.IsLocked(r.Id, out _))];

    public KeyValuePair<int, int>[] GetUnlockedStats()
    {
        var recipesByRarity = recipeRegistry.RecipesByRarity;
        var result = new KeyValuePair<int, int>[recipesByRarity.Length + 1];

        int total = 0, totalUnlocked = 0;

        for (int i = 0; i < recipesByRarity.Length; i++)
        {
            var recipes = recipesByRarity[i];
            var count = 0;

            foreach (var recipe in recipes)
            {
                if (!locker.IsLocked(recipe.Id, out _))
                {
                    count++;
                }
            }
            
            result[i] = new(count, recipes.Length);
            total += recipes.Length;
            totalUnlocked += count;
        }

        result[^1] = new(totalUnlocked, total);

        return result;
    }

    public void Unlock(string id)
    {
        locker.Unlock(id);
        persistentUnlocker.Add(id);
    }

}
