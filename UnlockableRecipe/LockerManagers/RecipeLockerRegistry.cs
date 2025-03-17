namespace UnlockableRecipe;

public class RecipeLockerRegistry(
    IEnumerable<ICustomRecipeLocker> registeredLockers,
    ILoc t
) : ILoadableSingleton, IUnloadableSingleton
{
    public ILoc T { get; } = t;

    internal static RecipeLockerRegistry? Instance { get; private set; }
    FrozenDictionary<string, IReadOnlyList<ICustomRecipeLocker>> lockers = null!;

    public void Load()
    {
        Instance = this;
        
        LoadLockers();
    }

    void LoadLockers()
    {
        Dictionary<string, List<ICustomRecipeLocker>> lockers = [];
        foreach (var blocker in registeredLockers)
        {
            foreach (var recipeId in blocker.MayLockRecipeIds)
            {
                if (!lockers.TryGetValue(recipeId, out List<ICustomRecipeLocker>? list))
                {
                    lockers[recipeId] = list = [];
                }
                list.Add(blocker);
            }
        }

        this.lockers = lockers.ToFrozenDictionary(
            q => q.Key,
            q => (IReadOnlyList<ICustomRecipeLocker>)q.Value);
    }

    public bool IsRecipeLocked(string recipeId) => IsRecipeBlocked(recipeId, null) is not null;
    public string? IsRecipeBlocked(string recipeId, ILoc? t)
    {
        if (!lockers.TryGetValue(recipeId, out var recipeBlockers)) { return null; }

        foreach (var blocker in recipeBlockers)
        {
            var shouldBlock = blocker.ShouldLockRecipe(recipeId, t);
            if (shouldBlock is not null) { return shouldBlock; }
        }

        return null;
    }

    public void Unload()
    {
        Instance = null;
    }
}
