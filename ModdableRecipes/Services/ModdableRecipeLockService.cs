namespace ModdableRecipes.Services;

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

            if (manufactory.CurrentRecipe?.Id == id)
            {
                manufactory.SetRecipe(null);
            }
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

    public void ReselectManufactory()
    {
        var selecting = selectionService.SelectedObject;

        if (selecting && selecting.HasComponent<Manufactory>())
        {
            selectionService.Unselect();
            selectionService.Select(selecting);
        }
    }

    public void Unlock(string id)
    {
        lockedRecipes.Remove(id);
        eb.Post(new ModdableRecipeUnlockedEvent(id));
        ReselectManufactory();
        OnRecipeStatusChanged();
    }

    public bool IsLocked(string id, [NotNullWhen(true)] out string? reason) => lockedRecipes.TryGetValue(id, out reason);

    public ModdableRecipeLockStatus GetLockStatus(string id) => lockedRecipes.ContainsKey(id)
        ? (ModdableRecipeLockStatus)(int)(specs.GetTitleVisibility(id) + 1)
        : ModdableRecipeLockStatus.Unlocked;

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
