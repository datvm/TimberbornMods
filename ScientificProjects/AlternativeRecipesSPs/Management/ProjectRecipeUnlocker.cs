global using ScientificProjects.Management;

namespace AlternativeRecipesSPs.Management;

public class ProjectRecipeUnlocker(EventBus eb, DefaultRecipeLockerController unlocker) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        if (ev.Project.Id == ModUtils.TimberbotId)
        {
            foreach (var id in ModUtils.TimberbotRecipes)
            {
                unlocker.Unlock(id);
            }
        }
        else if (ev.Project.IsAlternativeRecipe())
        {
            unlocker.Unlock(ev.Project.Id);
        }
    }

}
