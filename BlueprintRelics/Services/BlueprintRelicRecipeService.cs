namespace BlueprintRelics.Services;

public class BlueprintRelicRecipeService(
    BlueprintRelicRecipeRegistry recipeRegistry,
    ModdableRecipeLockService locker
)
{


    public IReadOnlyList<BlueprintRelicRecipePair>[] GetLockedRecipesByRarity() => [
        .. recipeRegistry.RecipesByRarity
            .Select(recipes => recipes
                .Where(r => locker.IsLocked(r.Id, out _))
                .ToArray())
    ];

    public void Unlock(string id) => locker.Unlock(id);

}
