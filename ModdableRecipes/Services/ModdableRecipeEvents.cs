namespace ModdableRecipes.Services;

public readonly record struct ModdableRecipeUnlockedEvent(string RecipeId);
public readonly record struct ModdableRecipeLockedEvent(string RecipeId, string Reason);
public readonly record struct RecipeModifiedEvent(string Id, RecipeSpec OldValues, RecipeSpec NewValues);