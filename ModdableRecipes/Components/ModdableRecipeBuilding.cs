namespace ModdableRecipes.Components;

[AddTemplateModule2(typeof(ModdableManufactoryDropdownProvider))]
public class ModdableRecipeBuilding : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(ModdableRecipeBuilding));
    static readonly ListKey<string> UnlockedRecipesKey = new("UnlockedRecipes");

    readonly HashSet<string> unlockedRecipes = [];

    public bool IsLocallyUnlocked(string recipeId) => unlockedRecipes.Contains(recipeId);

    public void Unlock(string recipeId) => unlockedRecipes.Add(recipeId);

    public void Relock(string recipeId) => unlockedRecipes.Remove(recipeId);

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        unlockedRecipes.Clear();
        if (s.Has(UnlockedRecipesKey))
        {
            unlockedRecipes.AddRange(s.Get(UnlockedRecipesKey));
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (unlockedRecipes.Count == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(UnlockedRecipesKey, unlockedRecipes);
    }

}
