global using ScientificProjects.Management;

namespace AlternativeRecipesSPs.Management;

public class ProjectRecipeUnlocker(EventBus eb, DefaultRecipeLockerController unlocker,  ScientificProjectService projects) : ILoadableSingleton
{

    public void Load()
    {
        EnsureUnlocked();
        eb.Register(this);
    }

    void EnsureUnlocked()
    {
        var unlockedProjects = projects.AllProjects
            .Where(q => q.IsAlternativeRecipe() && projects.IsUnlocked(q.Id));

        foreach (var project in unlockedProjects)
        {
            UnlockRecipeFor(project);
        }
    }

    [OnEvent]
    public void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        UnlockRecipeFor(ev.Project);
    }

    public static IEnumerable<string> GetUnlockingRecipes(ScientificProjectSpec proj)
    {
        if (proj.Id == ModUtils.TimberbotId)
        {
            foreach (var id in ModUtils.TimberbotRecipes)
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
        foreach (var id in GetUnlockingRecipes(proj))
        {
            unlocker.Unlock(id);
        }
    }

}
