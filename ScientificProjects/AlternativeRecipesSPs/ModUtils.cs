namespace AlternativeRecipesSPs;

internal static class ModUtils
{

    public const string RecipePrefix = "AR.";
    public const int RecipePrefixLength = 3;

    public const string TimberbotId = "TimberbotFramework";
    public static readonly ImmutableHashSet<string> TimberbotRecipes = ["AR.TimberbotChassis", "AR.TimberbotHeads"];

    public static bool IsAlternativeRecipePrefix(this ScientificProjectSpec spec) => spec.Id.StartsWith(RecipePrefix);
    public static bool IsAlternativeRecipe(this ScientificProjectSpec spec) => spec.Id == TimberbotId || spec.IsAlternativeRecipePrefix();
}
