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
        var recipeProjects = registry.AllProjects
            .Where(q => q.IsAlternativeRecipePrefix());

        ScientificProjectsUtils.Log(() =>
            string.Join(Environment.NewLine, recipeProjects.Select(q => q.ScienceCost + " Science to " + q.Effect)));

        var result = recipeProjects
            .Select(q => q.Id)
            .Concat(ModUtils.TimberbotRecipes)
            .ToImmutableHashSet();

        return result;
    }


}
