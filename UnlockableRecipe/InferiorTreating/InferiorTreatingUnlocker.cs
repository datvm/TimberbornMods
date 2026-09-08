
namespace UnlockableRecipe.InferiorTreating;

public class InferiorTreatingUnlocker(
    DefaultRecipeLockerController locker,
    BuildingUnlockingService buildingUnlocker,
    EventBus eb
) : ILoadableSingleton
{
    const string RecipeId = UnlockableRecipeModUtils.InferiorTreatedPlankRecipeId;

    public void Load()
    {
        if (locker.IsUnlocked(RecipeId))
        {
            return;
        }

        // Unlock if needed
        if (UnlockableRecipeModUtils.TapperShackPrefabNames
            .Any(buildingUnlocker._unlockedBuildings.Contains))
        {
            UnlockInferiorTreating();
            return;
        }

        eb.Register(this);
    }

    [OnEvent]
    public void OnBuildingUnlocked(BuildingUnlockedEvent ev)
    {
        var prefabSpec = ev.BuildingSpec.GetComponentFast<PrefabSpec>();

        if (prefabSpec is null) { return; }

        if (UnlockableRecipeModUtils.TapperShackPrefabNames.Contains(prefabSpec.PrefabName))
        {
            UnlockInferiorTreating();
            eb.Unregister(this);
        }
    }

    void UnlockInferiorTreating()
    {
        locker.Unlock(RecipeId);
    }

}
