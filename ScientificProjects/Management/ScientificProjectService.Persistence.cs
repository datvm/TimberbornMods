namespace ScientificProjects.Management;

partial class ScientificProjectService
{
    static readonly SingletonKey SaveKey = new("ScientificProjects");
    static readonly ListKey<string> UnlockedProjectsKey = new("UnlockedProjects");
    static readonly ListKey<string> ProjectLevelsKey = new("ProjectLevels");
    static readonly ListKey<string> TodayProjectLevelsKey = new("TodayProjectLevels");

    public void Load()
    {
        LoadSavedData();

        eb.Register(this);
    }

    void LoadSavedData()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }
        var s = loader.GetSingleton(SaveKey);

        unlockedProjects = [.. s.Get(UnlockedProjectsKey)];

        LoadSavedDict(s, ProjectLevelsKey, levels);
        FixSavedDataIfNeeded(levels, "Tomorrow levels");

        LoadSavedDict(s, TodayProjectLevelsKey, todayLevels);
        FixSavedDataIfNeeded(todayLevels, "Today levels");
    }

    void LoadSavedDict(IObjectLoader loader, ListKey<string> key, Dictionary<string, int> dict)
    {
        dict.Clear();

        if (!loader.Has(key)) { return; }

        foreach (var l in loader.Get(key))
        {
            var parts = l.Split(';');
            dict[parts[0]] = int.Parse(parts[1]);
        }
    }

    void FixSavedDataIfNeeded(Dictionary<string, int> projectLevels, string logDictName)
    {
        foreach (var (id, level) in projectLevels.ToList())
        {
            if (!TryGetProjectSpec(id, out var spec))
            {
                Debug.LogWarning($"Project {id} not found.");
                projectLevels.Remove(id);
                continue;
            }

            if (!spec.HasSteps)
            {
                Debug.LogWarning($"Project {id} has no steps. Possibly due to an updated mod");
                projectLevels.Remove(id);
                continue;
            }

            if (level > spec.MaxSteps)
            {
                Debug.LogWarning($"Project {id} has {nameof(spec.MaxSteps)} = {spec.MaxSteps}. Level {level} from {logDictName} is invalid and will be turned down.");
                projectLevels[id] = spec.MaxSteps;
            }

            if (level > 0 && !IsUnlocked(id))
            {
                Debug.LogWarning($"Project {id} is not unlocked but has level {level} from {logDictName}. Level will be set to 0.");
                projectLevels[id] = 0;
            }
        }
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(SaveKey);

        s.Set(UnlockedProjectsKey, unlockedProjects);
        SaveDict(s, ProjectLevelsKey, levels);
        SaveDict(s, TodayProjectLevelsKey, todayLevels);
    }

    void SaveDict(IObjectSaver saver, ListKey<string> key, Dictionary<string, int> dict)
    {
        saver.Set(key, [.. dict.Select(q => $"{q.Key};{q.Value}")]);
    }

    public void Unload()
    {
        eb.Unregister(this);
    }
}
