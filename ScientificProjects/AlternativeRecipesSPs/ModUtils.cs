namespace AlternativeRecipesSPs;

internal static class ModUtils
{

    public const string RecipePrefix = "AR.";
    public const int RecipePrefixLength = 3;

    public static bool IsAlternativeRecipe(this ScientificProjectSpec spec)
    {
        var result = spec.Id.StartsWith(RecipePrefix);

        Debug.Log($"IsAlternativeRecipe({spec.Id}): {result}");

        return result;
    }
}
