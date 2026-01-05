namespace ModdableRecipes.Services;

public class ModdableRecipeLockInitializer(
    ModdableRecipePersistentUnlocker persistentUnlocker,
    ModdableRecipeLockService locker,
    IEnumerable<IRecipeLockProvider> providers
) : ILoadableSingleton
{

    public void Load()
    {
        foreach (var provider in providers)
        {
            foreach (var (id, loc) in provider.GetLockedRecipes())
            {
                if (persistentUnlocker.IsUnlocked(id)) { continue; }
                locker.Lock(id, loc);
            }
        }
    }

}
