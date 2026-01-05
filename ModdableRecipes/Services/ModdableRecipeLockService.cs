namespace ModdableRecipes.Services;

public class ModdableRecipeLockService(
    EventBus eb,
    ModdableRecipeLockSpecService specs,
    EntityRegistry entityRegistry,
    EntitySelectionService selectionService,
    ILoc t
)
{

    readonly Dictionary<string, string> lockedRecipes = [];

    public void Lock(string id, string? reason) => Lock(id, reason ?? t.T("LV.MRec.DefaultReason"), false);

    internal void Lock(string id, string reason, bool doNotNotify)
    {
        if (!specs.ModdableRecipeById.ContainsKey(id))
        {
            throw new ArgumentException($"Recipe {id} is does not have {nameof(ModdableRecipeLockSpec)} attached. Attach one to its blueprint to lock it.");
        }

        if (lockedRecipes.ContainsKey(id))
        {
            throw new ArgumentException($"Recipe {id} is already locked with reason: {reason}");
        }

        lockedRecipes[id] = reason;

        UnselectRecipes(id);

        if (!doNotNotify)
        {
            eb.Post(new ModdableRecipeLockedEvent(id, reason));
        }
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
    }

    public bool IsLocked(string id, [NotNullWhen(true)] out string? reason) => lockedRecipes.TryGetValue(id, out reason);

    public ModdableRecipeLockStatus GetLockStatus(string id) => lockedRecipes.ContainsKey(id)
        ? (ModdableRecipeLockStatus)(int)(specs.GetTitleVisibility(id) + 1)
        : ModdableRecipeLockStatus.Unlocked;

}

public enum ModdableRecipeLockStatus
{
    Unlocked,
    Hidden,
    Censor,
    LockedVisible
}
