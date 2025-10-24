namespace ScientificProjects.Services;

public abstract class SimpleProjectListener : IScientificProjectUnlockListener, ILoadableSingleton
{

    public abstract string ProjectId { get; }
    public bool IsUnlocked => ProjectInfo is not null;
    public ScientificProjectInfo? ProjectInfo { get; private set; }

    public FrozenSet<string> UnlockListenerIds { get; private set; } = [];
    public FrozenSet<string> ListenerIds { get; private set; } = [];

    public virtual void Load()
    {
        UnlockListenerIds = ListenerIds = [ProjectId];
    }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        if (activeProjects.Count > 0)
        {
            UnlockProject(activeProjects[0]);
        }
    }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects) 
        => UnlockProject(activeProjects[0]);

    void UnlockProject(ScientificProjectInfo info)
    {
        ProjectInfo = info;
        OnProjectUnlocked();
    }

    public virtual void OnProjectUnlocked() { }

}
