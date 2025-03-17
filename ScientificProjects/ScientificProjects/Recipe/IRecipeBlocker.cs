namespace ScientificProjects.Recipe;

public interface IRecipeBlocker
{

    public ImmutableHashSet<string> MayBlockRecipeIds { get; }
    public string? ShouldBlockRecipe(string id);

}
