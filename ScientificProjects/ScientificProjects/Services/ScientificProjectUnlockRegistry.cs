namespace ScientificProjects.Services;

public class ScientificProjectUnlockRegistry(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    HashSet<string> unlockedProjects = [];
    public IEnumerable<string> UnlockedProjectIds => unlockedProjects;

    static readonly ListKey<string> UnlockedProjectsKey = new("UnlockedProjects");

    public bool Contains(string id) => unlockedProjects.Contains(id);

    public void Unlock(string id) => unlockedProjects.Add(id);

    public void Remove(string id) => unlockedProjects.Remove(id);

    public void Clear() => unlockedProjects.Clear();

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(ScientificProjectsUtils.SaveKey, out var s)) { return; }

        unlockedProjects = [.. s.Get(UnlockedProjectsKey)];
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(ScientificProjectsUtils.SaveKey);

        s.Set(UnlockedProjectsKey, unlockedProjects);
    }
}
