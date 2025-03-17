namespace ScientificProjects.Recipe;

public class RecipeBlockerRegistry(IEnumerable<IRecipeBlocker> registeredBlockers) : ILoadableSingleton
{

    FrozenDictionary<string, IReadOnlyList<IRecipeBlocker>> blockers = null!;

    public void Load()
    {
        Dictionary<string, List<IRecipeBlocker>> blockers = [];
        foreach (var blocker in registeredBlockers)
        {
            foreach (var recipeId in blocker.MayBlockRecipeIds)
            {
                if (!blockers.TryGetValue(recipeId, out List<IRecipeBlocker>? list))
                {
                    blockers[recipeId] = list = [];
                }
                list.Add(blocker);
            }
        }

        this.blockers = blockers.ToFrozenDictionary(q => q.Key, q => (IReadOnlyList<IRecipeBlocker>)q.Value);
    }

    public string? IsRecipeBlocked(string recipeId)
    {
        if (!blockers.TryGetValue(recipeId, out var recipeBlockers)) { return null; }

        foreach (var blocker in recipeBlockers)
        {
            var shouldBlock = blocker.ShouldBlockRecipe(recipeId);
            if (shouldBlock is not null) { return shouldBlock; }
        }

        return null;
    }

}
