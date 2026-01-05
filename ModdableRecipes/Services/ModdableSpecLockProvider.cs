namespace ModdableRecipes.Services;

public class ModdableSpecLockProvider(ModdableRecipeLockSpecService specs) : IRecipeLockProvider
{

    public IEnumerable<RecipeLock> GetLockedRecipes()
    {
        foreach (var (recipe, moddable) in specs.ModdableRecipes)
        {
            if (moddable.UnlockByDefault) { continue; }

            yield return new RecipeLock(recipe.Id, moddable.Description?.Value);
        }
    }

}
