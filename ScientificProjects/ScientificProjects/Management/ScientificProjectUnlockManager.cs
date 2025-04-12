namespace ScientificProjects.Management;

public class ScientificProjectUnlockManager(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    HashSet<string> unlockedProjects = [];
    public IEnumerable<string> UnlockedProjectIds => unlockedProjects;

    static readonly ListKey<string> UnlockedProjectsKey = new("UnlockedProjects");

    public bool Contains(string id) => unlockedProjects.Contains(id);

    public void Unlock(string id) => unlockedProjects.Add(id);

    public void Clear() => unlockedProjects.Clear();

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(ScientificProjectService.SaveKey, out var s)) { return; }

        unlockedProjects = [.. s.Get(UnlockedProjectsKey)];
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(ScientificProjectService.SaveKey);

        s.Set(UnlockedProjectsKey, unlockedProjects);
    }
}
