
namespace AlternativeRecipesSPs.Services;

public class ProjectRecipeUnlocker(
    DefaultRecipeLockerController recipeUnlocker, 
    ScientificProjectService projects,
    ScientificProjectUnlockService projectUnlocker
) : BaseProjectUpgradeListener(projectUnlocker)
{

    public override void Load()
    {
        base.Load();
        EnsureUnlocked();
    }

    void EnsureUnlocked()
    {
        foreach (var project in projects.ActiveProjects.Values)
        {
            UnlockRecipeFor(project.Spec);
        }
    }

    public static IEnumerable<string> GetUnlockingRecipes(ScientificProjectSpec proj)
    {
        if (proj.Id == ARSPUtils.TimberbotId)
        {
            foreach (var id in ARSPUtils.TimberbotRecipes)
            {
                yield return id;
            }
        }
        else if (proj.IsAlternativeRecipePrefix())
        {
            yield return proj.Id;
        }
    }

    void UnlockRecipeFor(ScientificProjectSpec proj)
    {
        if (!proj.IsAlternativeRecipe()) { return; }

        foreach (var id in GetUnlockingRecipes(proj))
        {
            recipeUnlocker.Unlock(id);
        }
    }

    protected override void OnProjectUnlocked(ScientificProjectSpec spec)
    {
        UnlockRecipeFor(spec);
    }
}
