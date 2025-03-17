namespace UnlockableRecipe;

public class RecipeAdder : RecipeModification
{

    public RecipeAdder Add(string prefabName, string recipeId)
    {
        AddToList(prefabName, recipeId);
        return this;
    }

}

public class RecipeRemover : RecipeModification
{

    public RecipeRemover Remove(string prefabName, string recipeId)
    {
        AddToList(prefabName, recipeId);
        return this;
    }

}

public class RecipeModification
{
    List<KeyValuePair<string, string>> RecipeList { get; } = [];
    public IReadOnlyList<KeyValuePair<string, string>> Recipes => RecipeList.AsReadOnly();

    protected void AddToList(string prefabName, string recipeId)
    {
        RecipeList.Add(new(prefabName, recipeId));
    }
}