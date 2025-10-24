namespace AlternativeRecipesSPs;

public static class ARSPUtils
{

    public const string RecipePrefix = "AR.";
    public const int RecipePrefixLength = 3;

    public const string TimberbotId = "TimberbotFramework";
    public static readonly FrozenSet<string> TimberbotRecipes = ["AR.TimberbotChassis", "AR.TimberbotHeads"];

    public static bool IsAlternativeRecipePrefix(this ScientificProjectSpec spec) => spec.Id.StartsWith(RecipePrefix);
    public static bool IsAlternativeRecipe(this ScientificProjectSpec spec) => spec.Id == TimberbotId || spec.IsAlternativeRecipePrefix();
}
