namespace UnlockableRecipe;

/// <summary>
/// Controller for default recipe lockers that are just locked/unlocked through this controller.
/// </summary>
public class DefaultRecipeLockerController(
    IEnumerable<IDefaultRecipeLocker> defaultLockers,
    ISingletonLoader loader,
    EventBus eb
) : ICustomRecipeLocker, ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("UnlockableRecipe");
    static readonly ListKey<string> UnlockedRecipesKey = new("UnlockedRecipes");

    HashSet<string> unlockedRecipes = [];
    FrozenDictionary<string, IDefaultRecipeLocker> lockers = null!;

    public ImmutableHashSet<string> MayLockRecipeIds { get; private set; } = null!;

    /// <summary>
    /// Unlock a recipe. Also raise an event if the recipe was previously locked.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to unlock.</param>
    /// <returns>True if the recipe was previously locked, false if it was already unlocked.</returns>
    /// <exception cref="InvalidOperationException">If a recipe is not registered with a <see cref="IDefaultRecipeLocker"/>.</exception>
    public bool Unlock(string recipeId)
    {
        ValidateId(recipeId);

        if (unlockedRecipes.Contains(recipeId)) { return false; }

        unlockedRecipes.Add(recipeId);
        eb.Post(new OnRecipeUnlocked(recipeId));
        return true;
    }

    /// <summary>
    /// Lock a recipe again. Also raise an event if the recipe was previously unlocked.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to lock.</param>
    /// <returns>True if the recipe was previously unlocked, false if it was already locked.</returns>
    /// <exception cref="InvalidOperationException">If a recipe is not registered with a <see cref="IDefaultRecipeLocker"/>.</exception>
    public bool Lock(string recipeId)
    {
        ValidateId(recipeId);
        if (!unlockedRecipes.Contains(recipeId)) { return false; }

        unlockedRecipes.Remove(recipeId);
        eb.Post(new OnRecipeLocked(recipeId));
        return true;
    }

    void ValidateId(string recipeId)
    {
        if (!MayLockRecipeIds.Contains(recipeId))
        {
            throw new InvalidOperationException($"Recipe {recipeId} is not registered with a {nameof(IDefaultRecipeLocker)}.");
        }
    }

    public void Load()
    {
        MayLockRecipeIds = [.. defaultLockers.SelectMany(q => q.MayLockRecipeIds)];
        LoadLockers();
        LoadSavedData();
    }

    void LoadLockers()
    {
        Dictionary<string, IDefaultRecipeLocker> idLockers = [];

        foreach (var b in defaultLockers)
        {
            foreach (var id in b.MayLockRecipeIds)
            {
                if (idLockers.TryGetValue(id, out var b2))
                {
                    throw new InvalidOperationException($"Multiple Lockers for recipe {id}: {b.GetType().FullName} and {b2.GetType().FullName}");
                }

                idLockers[id] = b;
            }
        }

        lockers = idLockers.ToFrozenDictionary();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(UnlockedRecipesKey))
        {
            unlockedRecipes = [.. s.Get(UnlockedRecipesKey)];
        }
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(SaveKey);
        s.Set(UnlockedRecipesKey, unlockedRecipes);
    }

    /// <summary>
    /// Check if a recipe is unlocked.
    /// </summary>
    /// <param name="id">The ID of the recipe to check.</param>
    /// <returns>True if the recipe is unlocked, false if it's locked.</returns>
    public bool IsUnlocked(string id) => unlockedRecipes.Contains(id);

    /// <summary>
    /// Give the reason why a recipe is locked.
    /// </summary>
    /// <param name="id">The ID of the recipe to check.</param>
    /// <param name="t">The localization to use for the reason. If you do not provide one, any non-null string may be returned for locked results.</param>
    /// <returns>The reason why the recipe is blocked to show to user, or null if the recipe is unlocked.</returns>
    public string? ShouldLockRecipe(string id, ILoc? t)
    {
        if (IsUnlocked(id)) { return null; }

        return t is null ? "" : lockers[id].GetLockReasonFor(id, t);
    }

}
