namespace AlternativeRecipesSPs.Services;

public class ProjectRecipeDescriber(ScientificProjectRegistry projects, RecipeSpecService recipes, ILoc t) : ILoadableSingleton
{

    public void Load()
    {
        foreach (var proj in projects.AllProjects)
        {
            if (!proj.IsAlternativeRecipe()) { continue; }

            var id = ProjectRecipeUnlocker.GetUnlockingRecipes(proj).FirstOrDefault();
            if (id is null) { continue; }

            var recipe = recipes.GetRecipe(id);
            proj.Effect = proj.Effect.Replace("[Recipe]", t.T(recipe.DisplayLocKey));
        }
    }

}
