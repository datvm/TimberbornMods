namespace ModdableRecipes.Services;

[BindSingleton]
public class ModdableRecipeLockService(
    EventBus eb,
    ModdableRecipeLockSpecService specs,
    EntityRegistry entityRegistry,
    EntitySelectionService selectionService,
    ILoc t,
    ModdableRecipePersistentUnlocker persistentUnlocker,
    IEnumerable<IRecipeLockProvider> providers,
    DescriptionPanels descriptionPanels
) : ILoadableSingleton, IUnloadableSingleton
{

    readonly Dictionary<string, string> lockedRecipes = [];

    public static ModdableRecipeLockService? Instance { get; private set; }

    public void Load()
    {
        foreach (var provider in providers)
        {
            foreach (var (id, loc) in provider.GetLockedRecipes())
            {
                if (persistentUnlocker.IsUnlocked(id)) { continue; }
                Lock(id, loc, true);
            }
        }

        Instance = this;
    }

    public void Lock(string id, string? reason) => Lock(id, reason, false);

    internal void Lock(string id, string? reason, bool doNotNotify)
    {
        if (!specs.ModdableRecipeById.ContainsKey(id))
        {
            throw new ArgumentException($"Recipe {id} is does not have {nameof(ModdableRecipeLockSpec)} attached. Attach one to its blueprint to lock it.");
        }

        if (lockedRecipes.ContainsKey(id))
        {
            throw new ArgumentException($"Recipe {id} is already locked with reason: {reason}");
        }

        reason ??= t.T("LV.MRec.DefaultReason");
        lockedRecipes[id] = reason;

        UnselectRecipes(id);

        if (!doNotNotify)
        {
            eb.Post(new ModdableRecipeLockedEvent(id, reason));
        }

        OnRecipeStatusChanged();
    }

    void UnselectRecipes(string id)
    {
        foreach (var e in entityRegistry.Entities)
        {
            var manufactory = e.GetComponent<Manufactory>();
            if (!manufactory) { continue; }

            if (manufactory.CurrentRecipe?.Id != id) { continue; }

            var building = e.GetComponent<ModdableRecipeBuilding>();
            if (building && building.IsLocallyUnlocked(id)) { continue; }

            manufactory.SetRecipe(null);
        }

        ReselectManufactory();
    }

    void OnRecipeStatusChanged()
    {
        RefreshDescriptionPanels();
    }

    void RefreshDescriptionPanels()
    {
        foreach (var p in descriptionPanels._descriptionPanels.Values)
        {
            p.Root.RemoveFromHierarchy();
        }

        descriptionPanels._descriptionPanels.Clear();
    }

    public async void ReselectManufactory()
    {
        var selectingObj = selectionService.SelectedObject;
        var manufactory = selectingObj ? selectingObj.GetComponent<Manufactory>() : null;

        if (!manufactory) { return; }

        await Awaitable.NextFrameAsync();
        selectionService.Unselect();

        await Awaitable.NextFrameAsync();
        selectionService.Select(manufactory); // No need to validate, the Select method already validate the object
    }

    public void Unlock(string id)
    {
        lockedRecipes.Remove(id);
        eb.Post(new ModdableRecipeUnlockedEvent(id));
        ReselectManufactory();
        OnRecipeStatusChanged();
    }

    public void UnlockLocally(ModdableRecipeBuilding building, string id)
    {
        building.Unlock(id);
        ReselectManufactory();
        OnRecipeStatusChanged();
    }

    public void RelockLocally(ModdableRecipeBuilding building, string id)
    {
        building.Relock(id);

        var manufactory = building.GetComponent<Manufactory>();
        if (manufactory && manufactory.CurrentRecipe?.Id == id && IsLocked(id, out _))
        {
            manufactory.SetRecipe(null);
        }

        ReselectManufactory();
        OnRecipeStatusChanged();
    }

    public bool IsLocked(string id, [NotNullWhen(true)] out string? reason) => lockedRecipes.TryGetValue(id, out reason);

    public bool IsLocked(string id, ModdableRecipeBuilding? building, [NotNullWhen(true)] out string? reason)
    {
        if (!lockedRecipes.TryGetValue(id, out reason)) { return false; }

        if (building is not null && building.IsLocallyUnlocked(id))
        {
            reason = null;
            return false;
        }

        return true;
    }

    public ModdableRecipeLockStatus GetLockStatus(string id) => lockedRecipes.ContainsKey(id)
        ? (ModdableRecipeLockStatus)(int)(specs.GetTitleVisibility(id) + 1)
        : ModdableRecipeLockStatus.Unlocked;

    public ModdableRecipeLockStatus GetLockStatus(string id, ModdableRecipeBuilding? building)
    {
        if (building is not null && building.IsLocallyUnlocked(id))
        {
            return ModdableRecipeLockStatus.Unlocked;
        }

        return GetLockStatus(id);
    }

    public void Unload()
    {
        Instance = null;
    }
}

public enum ModdableRecipeLockStatus
{
    Unlocked,
    Hidden,
    Censor,
    LockedVisible
}
