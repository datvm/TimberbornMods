namespace ScientificProjects.Services;

public class ScientificProjectGroupService(
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly ListKey<string> CollapsedGroupsKey = new("CollapsedGroups");
    readonly HashSet<string> collapsedGroupIds = [];

    public IEnumerable<string> CollapsedGroupIds => collapsedGroupIds;

    public bool ShouldCollapse(string groupId) => collapsedGroupIds.Contains(groupId);
    public void SetCollapsed(string groupId, bool collapsed)
    {
        if (collapsed)
        {
            collapsedGroupIds.Add(groupId);
        }
        else
        {
            collapsedGroupIds.Remove(groupId);
        }
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(ScientificProjectsUtils.SaveKey, out var s)) { return; }

        if (s.Has(CollapsedGroupsKey))
        {
            collapsedGroupIds.AddRange(s.Get(CollapsedGroupsKey));
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(ScientificProjectsUtils.SaveKey);
        s.Set(CollapsedGroupsKey, collapsedGroupIds);
    }
}
