namespace AlternativeRecipesSPs.Services;

public class ProjectRecipeDescriber(
    ScientificProjectRegistry registry,
    RecipeSpecService recipes,
    ILoc t
) : ILoadableSingleton
{

    public void Load()
    {
#warning Delete after debug
        StringBuilder str = new();

        foreach (var proj in registry.AllProjects)
        {
            if (!proj.IsAlternativeRecipe()) { continue; }

            var id = ProjectRecipeUnlocker.GetUnlockingRecipes(proj).FirstOrDefault();
            if (id is null) { continue; }

            var recipe = recipes.GetRecipe(id);
            proj.Effect = proj.Effect.Replace("[Recipe]", t.T(recipe.DisplayLocKey));

            str.AppendLine($"- {proj.DisplayName} [{proj.ScienceCost} Science]: {proj.Effect}");
        }

        Debug.Log(str.ToString());
    }

}
