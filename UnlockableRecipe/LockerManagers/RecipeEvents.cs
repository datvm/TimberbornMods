namespace UnlockableRecipe;

public readonly record struct OnRecipeUnlocked(string RecipeId);
public readonly record struct OnRecipeLocked(string RecipeId);
