namespace AlternativeRecipesSPs.Services;

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
        var recipeProjects = registry.AllProjects
            .Where(q => q.IsAlternativeRecipePrefix());

        var result = recipeProjects
            .Select(q => q.Id)
            .Concat(ARSPUtils.TimberbotRecipes)
            .ToImmutableHashSet();

        return result;
    }


}
