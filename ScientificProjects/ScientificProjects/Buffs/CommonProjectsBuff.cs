namespace ScientificProjects.Buffs;

public abstract class CommonProjectsBuff(
    ISingletonLoader loader,
    IBuffService buffs,
    EventBus eb,
    ScientificProjectService projects
) : SimpleBuff(loader, buffs)
{
    protected internal readonly IBuffService buffs = buffs;
    protected readonly EventBus eb = eb;
    protected readonly ScientificProjectService projects = projects;

    public override string Name { get; protected set; } = ""; // Will be determine by the buff instance
    public override string Description { get; protected set; } = ""; // Will be determine by the buff instance

    protected abstract HashSet<string> SupportedOneTimeIds { get; }
    protected abstract HashSet<string> SupportedDailyIds { get; }

    protected IReadOnlyList<ScientificProjectInfo> UnlockedSupportedProjects
        => GetUnlockedOrActiveProjects(SupportedOneTimeIds);

    protected IReadOnlyList<ScientificProjectInfo> ActiveDailySupportedProjects
        => GetUnlockedOrActiveProjects(SupportedDailyIds);

    protected override void AfterLoad()
    {
        base.AfterLoad();

        eb.Register(this);

        RefreshOnetimeBuffs(null);
        RefreshDailyBuffs();
    }

    protected abstract void RefreshOnetimeBuffs(ScientificProjectSpec? justUnlocked);

    protected abstract IEnumerable<Type> DailyBuffInstanceTypes { get; }
    protected virtual void RefreshDailyBuffs()
    {
        // Remove all of them
        foreach (var type in DailyBuffInstanceTypes)
        {
            RemoveBuffInstances(type);
        }

        // Get projects
        ProcessDailyBuffs(ActiveDailySupportedProjects);
    }
    protected abstract void ProcessDailyBuffs(IEnumerable<ScientificProjectInfo> activeProjects);

    protected void RemoveBuffInstances(Type type)
    {
        if (!typeof(BuffInstance).IsAssignableFrom(type))
        {
            throw new ArgumentException("Type must be a BuffInstance", nameof(type));
        }

        var method = typeof(IBuffService).GetMethod(nameof(IBuffService.RemoveAllInstances));
        var generic = method.MakeGenericMethod(type);
        generic.Invoke(buffs, []);
    }

    protected void RemoveBuffInstances<T>()
        where T : BuffInstance
    {
        buffs.RemoveAllInstances<T>();
    }

    [OnEvent]
    public virtual void OnProjectUnlocked(OnScientificProjectUnlockedEvent e)
    {
        if (!SupportedOneTimeIds.Contains(e.Project.Id)) { return; }

        RefreshOnetimeBuffs(e.Project);
    }

    [OnEvent]
    public virtual void OnProjectDailyPaid(OnScientificProjectDailyCostChargedEvent _)
    {
        RefreshDailyBuffs();
    }

    protected IReadOnlyList<ScientificProjectInfo> GetUnlockedOrActiveProjects(IEnumerable<string> ids) 
        => [.. ids
            .Select(projects.GetProject)
            .Where(p => p.Unlocked && (!p.Spec.HasSteps || p.TodayLevel > 0))];

}