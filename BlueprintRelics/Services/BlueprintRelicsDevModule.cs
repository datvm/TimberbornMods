namespace BlueprintRelics.Services;

[MultiBind(typeof(IDevModule))]
public class BlueprintRelicsDevModule(
    BlueprintRelicsSpawner spawner,
    EntitySelectionService selectionService,
    BlueprintRelicRecipeService recipes,
    ModdableRecipePersistentUnlocker persistentUnlocker
) : IDevModule
{
    public DevModuleDefinition GetDefinition() => new DevModuleDefinition.Builder()
        .AddMethod(DevMethod.Create("Blueprint Relics: Try spawning", TrySpawn))
        .AddMethod(DevMethod.Create("Blueprint Relics: Guaranteed spawn", GuaranteedSpawn))
        .AddMethod(DevMethod.Create("Blueprint Relics: Finish selecting relic", FinishCurrent))
        .AddMethod(DevMethod.Create("Blueprint Relics: Unlock all recipes", UnlockAllRecipes))
        .AddMethod(DevMethod.Create("Blueprint Relics: Lock all recipes", LockAllRecipes))
        .Build();

    void TrySpawn() => spawner.TryToSpawn();
    void GuaranteedSpawn() => spawner.Spawn();

    void UnlockAllRecipes()
    {
        var lockedRecipes = recipes.GetLockedRecipesByRarity();
        foreach (var list in lockedRecipes)
        {
            foreach (var recipe in list)
            {
                recipes.Unlock(recipe.Id);
            }
        }
    }

    void LockAllRecipes()
    {
        var allRecipes = recipes.GetUnlockedRecipes();
        foreach (var r in allRecipes)
        {
            persistentUnlocker.Remove(r.Id);
        }
    }

    void FinishCurrent()
    {
        var curr = selectionService.SelectedObject;
        if (!curr) { return; }

        var collector = curr.GetComponent<BlueprintRelicCollector>();
        if (!collector) { return; }

        collector.DevFinishExcavation();
    }

}
