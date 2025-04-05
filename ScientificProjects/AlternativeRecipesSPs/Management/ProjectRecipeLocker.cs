global using ScientificProjects.Specs;

namespace AlternativeRecipesSPs.Management;

public class ProjectRecipeLocker(ILoc t, ScientificProjectRegistry registry) : IDefaultRecipeLocker, ILoadableSingleton
{
    readonly string lockReason = t.T("LV.ARSP.LockReason");

    public ImmutableHashSet<string> MayLockRecipeIds { get; private set; } = null!;

    public string GetLockReasonFor(string id, ILoc t) => lockReason;

    public void Load()
    {
        MayLockRecipeIds = GetLockedRecipeIds(registry);
    }

    static ImmutableHashSet<string> GetLockedRecipeIds(ScientificProjectRegistry registry)
    {
        var result = registry.AllProjects
            .Where(q => q.IsAlternativeRecipe())
            .Select(q => q.Id)
            .ToImmutableHashSet();

        Debug.Log("Alternative Recipes: " + string.Join(", ", result));

        return result;
    }


}
