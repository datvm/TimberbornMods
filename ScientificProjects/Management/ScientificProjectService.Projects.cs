
namespace ScientificProjects.Management;

partial class ScientificProjectService
{

    public bool TryGetProjectSpec(string id, [MaybeNullWhen(false)] out ScientificProjectSpec spec)
    {
        return registry.projectsById.TryGetValue(id, out spec);
    }

    public ScientificProjectSpec GetProjectSpec(string id) => registry.GetProject(id);
    public IEnumerable<ScientificProjectSpec> AllProjects => registry.AllProjects;
    public IEnumerable<ScientificProjectGroupSpec> AllGroups => registry.AllGroups;

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

    public ScientificProjectInfo GetProject(string id) => 
        projectInfoCache?.TryGetValue(id, out var proj) == true 
        ? proj 
        : GetProject(GetProjectSpec(id));

    public ScientificProjectInfo GetProject(ScientificProjectSpec spec)
    {
        if (projectInfoCache?.TryGetValue(spec.Id, out var proj) == true) { return proj; }

        var level = GetLevel(spec.Id);
        var todayLevel = GetTodayLevel(spec.Id);
        var unlocked = IsUnlocked(spec.Id);
        var preqProject = spec.RequiredId is null ? null : GetProject(spec.RequiredId);

        var result = new ScientificProjectInfo(spec, unlocked, level, todayLevel, preqProject);

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


}
