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
        if (ev.Project.IsAlternativeRecipe())
        {
            unlocker.Unlock(ev.Project.Id);
        }
    }

}
