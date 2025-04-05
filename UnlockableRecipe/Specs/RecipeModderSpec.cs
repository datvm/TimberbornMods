namespace UnlockableRecipe;

public record RecipeModderSpec : ComponentSpec
{

    [Serialize]
    public ImmutableArray<string> PrefabNames { get; init; } = [];

    [Serialize]
    public ImmutableArray<string> RecipeIds { get; init; } = [];

    [Serialize]
    public bool Remove { get; init; }

}
