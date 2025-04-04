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
        if (!loader.HasSingleton(ScientificProjectService.SaveKey)) { return; }
        var s = loader.GetSingleton(ScientificProjectService.SaveKey);

        unlockedProjects = [.. s.Get(UnlockedProjectsKey)];
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(ScientificProjectService.SaveKey);

        s.Set(UnlockedProjectsKey, unlockedProjects);
    }
}
