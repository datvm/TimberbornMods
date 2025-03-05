global using Timberborn.ScienceSystem;

namespace ScientificProjects.Management;

public record class ScientificProjectGroupInfo(ScientificProjectGroupSpec Spec, IEnumerable<ScientificProjectInfo> Projects);
public record class ScientificProjectInfo(ScientificProjectSpec Spec, bool Unlocked, int Level, ScientificProjectInfo? PreqProject);

public partial class ScientificProjectService(
    ISingletonLoader loader,
    ScientificProjectRegistry registry,
    EventBus eb,
    ScienceService sciences
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new("ScientificProjects");
    static readonly ListKey<string> UnlockedProjectsKey = new("UnlockedProjects");
    static readonly ListKey<string> ProjectLevelsKey = new("ProjectLevels");

    HashSet<string> unlockedProjects = [];
    readonly Dictionary<string, int> levels = [];

    public ScientificProjectSpec this[string id] => registry.GetProject(id);
    public IEnumerable<ScientificProjectSpec> AllProjects => registry.AllProjects;
    public IEnumerable<ScientificProjectGroupSpec> AllGroups => registry.AllGroups;

    public IEnumerable<string> UnlockedProjectIds => unlockedProjects;
    public bool IsUnlocked(string projectId)
    {
        if (unlockedProjects.Contains(projectId)) { return true; }

        var proj = this[projectId];
        return proj.HasSteps &&
            (proj.RequiredId is null || IsUnlocked(proj.RequiredId));
    }
    public bool IsPreqUnlocked(ScientificProjectSpec p) => p.RequiredId is null || IsUnlocked(p.RequiredId);

    public ScientificProjectUnlockStatus? TryToUnlock(string id, bool force = false)
    {
        if (!force)
        {
            var canUnlock = CanUnlock(id);

            if (canUnlock != ScientificProjectUnlockStatus.CanUnlock)
            {
                return canUnlock;
            }

            sciences.SubtractPoints(this[id].ScienceCost);
        }

        PerformUnlock(id);
        return null;
    }

    public int GetLevel(string id)
    {
        return levels.TryGetValue(id, out var l) ? l : 0;
    }

    Dictionary<string, ScientificProjectInfo>? projectInfoCache;
    T RunWithProjectCache<T>(Func<T> action)
    {
        projectInfoCache = [];
        var result = action();
        projectInfoCache = null;
        return result;
    }

    public IEnumerable<ScientificProjectInfo> GetAllProjects()
    {
        return RunWithProjectCache<IEnumerable<ScientificProjectInfo>>(() =>
        {
            return [.. registry.AllProjects.Select(GetProject)];
        });
    }

    public ScientificProjectInfo GetProject(string id) => projectInfoCache?.TryGetValue(id, out var proj) == true ? proj : GetProject(this[id]);

    public ScientificProjectInfo GetProject(ScientificProjectSpec spec)
    {
        if (projectInfoCache?.TryGetValue(spec.Id, out var proj) == true) { return proj; }

        var level = GetLevel(spec.Id);
        var unlocked = IsUnlocked(spec.Id);
        var preqProject = spec.RequiredId is null ? null : GetProject(spec.RequiredId);

        var result = new ScientificProjectInfo(spec, unlocked, level, preqProject);

        if (projectInfoCache is not null)
        {
            projectInfoCache[spec.Id] = result;
        }
        return result;
    }

    public IEnumerable<ScientificProjectGroupInfo> GetAllProjectGroups()
    {
        return RunWithProjectCache<IEnumerable<ScientificProjectGroupInfo>>(() =>
        {
            return [..AllGroups.Select(q => new ScientificProjectGroupInfo(
                q,
                [..registry.GetProjects(q.Id)
                    .Select(q => GetProject(q))])
            )];
        });
    }

    public ScientificProjectUnlockStatus CanUnlock(string id)
    {
        if (unlockedProjects.Contains(id))
        {
            return ScientificProjectUnlockStatus.Unlocked;
        }

        var proj = this[id];

        if (proj.HasSteps)
        {
            return ScientificProjectUnlockStatus.Unlocked;
        }

        if (proj.RequiredId is not null && !IsUnlocked(proj.RequiredId))
        {
            return ScientificProjectUnlockStatus.RequirementLocked;
        }

        // Unlockable projects cannot have scaling cost so at this step, only check for fixed cost
        return sciences.SciencePoints > proj.ScienceCost
            ? ScientificProjectUnlockStatus.CanUnlock
            : ScientificProjectUnlockStatus.CostLocked;
    }

    public void SetLevel(ScientificProjectSpec project, int level)
    {
        if (level < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "Level cannot be negative");
        }

        if (level > project.MaxSteps)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, $"Project {project.Id} has {nameof(project.MaxSteps)} = {project.MaxSteps}");
        }

        levels[project.Id] = level;
        eb.Post(new OnScientificProjectLevelChangeEvent(project, level));
    }

    public int GetCost(ScientificProjectSpec project)
    {
        return GetCost(GetProject(project));
    }

    /// <summary>
    /// Make sure the info instance is up-to-date. If not, call <see cref="GetCost(ScientificProjectSpec)"/> instead.
    /// </summary>
    public int GetCost(ScientificProjectInfo info)
    {
        if (info.Spec.HasScalingCost)
        {
            return registry.GetCostProviderFor(info.Spec.Id).CalculateCost(info);
        }
        else
        {
            return info.Spec.ScienceCost;
        }
    }


    void PerformUnlock(string id)
    {
        unlockedProjects.Add(id);

        eb.Post(new OnScientificProjectUnlockedEvent(this[id]));
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.HasSingleton(SaveKey)) { return; }
        var s = loader.GetSingleton(SaveKey);

        unlockedProjects = [.. s.Get(UnlockedProjectsKey)];

        var levels = s.Get(ProjectLevelsKey);
        foreach (var l in levels)
        {
            var parts = l.Split(';');
            this.levels[parts[0]] = int.Parse(parts[1]);
        }
    }

    public void Save(ISingletonSaver saver)
    {
        var s = saver.GetSingleton(SaveKey);

        s.Set(UnlockedProjectsKey, unlockedProjects);
        s.Set(ProjectLevelsKey, [.. levels.Select(q => $"{q.Key};{q.Value}")]);
    }

}

public enum ScientificProjectUnlockStatus
{
    CanUnlock,
    Unlocked,
    RequirementLocked,
    CostLocked,
}