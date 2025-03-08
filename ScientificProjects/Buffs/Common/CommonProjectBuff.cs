namespace ScientificProjects.Buffs;

public abstract class CommonProjectBuff<TBuff, TInstance>(
    ISingletonLoader loader,
    IBuffService buffs,
    ScientificProjectService projects,
    EventBus eb
) : SimpleValueBuff<IEnumerable<ScientificProjectInfo>, TBuff, TInstance>(loader, buffs)
    where TBuff : CommonProjectBuff<TBuff, TInstance>
    where TInstance : BuffInstance<IEnumerable<ScientificProjectInfo>, TBuff>, new()
{
    
    protected internal readonly IBuffService buffs = buffs;    
    protected internal readonly EventBus eb = eb;
    protected internal readonly ScientificProjectService projects = projects;

    protected abstract ImmutableHashSet<string> ProjectIds { get; }

    protected override void AfterLoad()
    {
        RefreshBuff();
        eb.Register(this);
    }

    protected virtual void RefreshBuff()
    {
        var relevantProjects = GetRelevantProjects();
        if (RemoveExistingBuffOnRefresh)
        {
            RemoveExistingBuff();
        }

        if (!relevantProjects.Any() && NoCreateIfNoProject) { return; }

        var instance = CreateProjectBuffInstance(relevantProjects);
        buffs.Apply(instance);
    }

    /// <summary>
    /// Get all projects that will be passed over to the buff instance
    /// </summary>
    protected virtual IEnumerable<ScientificProjectInfo> GetRelevantProjects()
    {
        var projectList = ProjectIds.Select(projects.GetProject)
            .OrderBy(q => q.Spec.Order);

        var unlockedList = projectList
            .Where(q => q.Unlocked)
            .ToImmutableArray();

        return unlockedList;
    }

    /// <summary>
    /// Indicate if the existing buff instance should be removed before creating a new one
    /// </summary>
    protected virtual bool RemoveExistingBuffOnRefresh => true;

    /// <summary>
    /// Remove all existing buffs of the instance
    /// </summary>
    protected virtual void RemoveExistingBuff()
    {
        RemoveExistingBuff<TInstance>();
    }

    protected void RemoveExistingBuff<T>() where T : BuffInstance
    {
        buffs.RemoveAllInstances<T>();
    }

    /// <summary>
    /// Indicate if the buff instance should not be created if there are no relevant projects
    /// </summary>
    protected virtual bool NoCreateIfNoProject => true;

    /// <summary>
    /// Create the instance of the buff from the relevant projects
    /// </summary>
    protected virtual TInstance CreateProjectBuffInstance(IEnumerable<ScientificProjectInfo> projects) => CreateInstance(projects);

    [OnEvent]
    public virtual void OnProjectUnlocked(OnScientificProjectUnlockedEvent ev)
    {
        if (!ProjectIds.Contains(ev.Project.Id)) { return; }

        RefreshBuff();
    }

}