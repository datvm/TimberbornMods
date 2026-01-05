namespace ModdableRecipes.Services;

public class ModdableRecipePersistentUnlocker(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ModdableRecipePersistentUnlocker));
    static readonly ListKey<string> UnlockedRecipesKey = new("UnlockedRecipes");

    readonly HashSet<string> unlockedRecipes = [];

    public void Add(string id) => unlockedRecipes.Add(id);
    public void Remove(string id) => unlockedRecipes.Remove(id);
    public bool IsUnlocked(string id) => unlockedRecipes.Contains(id);

    public void Load()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        unlockedRecipes.Clear();
        unlockedRecipes.AddRange(s.Get(UnlockedRecipesKey));
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(UnlockedRecipesKey, unlockedRecipes);
    }
}
